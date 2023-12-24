using System.Text.RegularExpressions;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

[DokiOutput("Doki.Output.Markdown")]
public sealed partial class MarkdownOutput(OutputContext context) : OutputBase<OutputOptions>(context)
{
    public override async Task WriteAsync(TableOfContents tableOfContents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableOfContents);

        var currentPath = tableOfContents.GetPath();

        var tableOfContentsFile =
            new FileInfo(Path.Combine(OutputDirectory.FullName, currentPath, "README.md"));

        if (!tableOfContentsFile.Directory!.Exists) tableOfContentsFile.Directory.Create();

        var markdown = new MarkdownBuilder(currentPath)
            .Add(new Heading(tableOfContents.Id, 1));

        if (tableOfContents.Properties?.TryGetValue("Description", out var description) == true)
        {
            markdown.Add(new Text(description?.ToString()!));
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (tableOfContents.Content)
        {
            case DokiContent.Assemblies:
                await BuildAssembliesTableOfContentsAsync(markdown, tableOfContents, cancellationToken);
                break;
            case DokiContent.Assembly:
                foreach (var child in tableOfContents.Children)
                {
                    if (child is not TableOfContents namespaceToC) continue;

                    await WriteAsync(namespaceToC, cancellationToken);
                }

                markdown.Add(new Heading("Namespaces", 2));
                markdown.Add(new List
                {
                    Items = tableOfContents.Children.Select(x =>
                        (Element) new Link(x.Id, markdown.BuildRelativePath(x) + "/README.md")).ToList()
                });
                break;
            case DokiContent.Namespace:
                markdown.Add(new Heading("Types", 2));
                goto default;
            default:
                markdown.Add(new List
                {
                    Items = tableOfContents.Children.Select(x => BuildMarkdownTableOfContents(markdown, x, 0)).ToList()
                });
                break;
        }

        await WriteMarkdownAsync(tableOfContentsFile, markdown, cancellationToken);
    }

    public override async Task WriteAsync(TypeDocumentation typeDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);

        var currentPath = typeDocumentation.GetPath(".md");

        var typeDocumentationFile = new FileInfo(Path.Combine(OutputDirectory.FullName, currentPath));

        if (!typeDocumentationFile.Directory!.Exists) typeDocumentationFile.Directory.Create();

        var name = typeDocumentation.Properties?.TryGetValue("Name", out var nameProperty) == true
            ? nameProperty?.ToString()!
            : typeDocumentation.Id;

        var markdown = new MarkdownBuilder(currentPath)
            .Add(new Heading(name, 1).Append($" {Enum.GetName(typeDocumentation.Content)}"))
            .Add(new Heading("Definition", 2));

        var namespaceToC = typeDocumentation.TryGetParent<TableOfContents>(DokiContent.Namespace);
        if (namespaceToC != null)
        {
            markdown.Add(new Text("Namespace: ")
                .Append(new Link(namespaceToC.Id, markdown.BuildRelativePath(namespaceToC) + "/README.md")));
        }

        var assemblyToC = typeDocumentation.TryGetParent<TableOfContents>(DokiContent.Assembly);
        if (assemblyToC != null)
        {
            var assemblyName = assemblyToC.Properties?.TryGetValue("FileName", out var assemblyNameProperty) == true
                ? assemblyNameProperty?.ToString()!
                : assemblyToC.Id;

            markdown.Add(new Text("Assembly: ")
                .Append(new Link(assemblyName, markdown.BuildRelativePath(assemblyToC) + "/README.md")));

            var packageId = assemblyToC.Properties?.TryGetValue("PackageId", out var packageIdProperty) == true
                ? packageIdProperty?.ToString()!
                : null;

            if (packageId != null)
            {
                markdown.Add(new Text("Package: ")
                    //TODO support other package sources
                    // ReSharper disable once UseStringInterpolation
                    .Append(new Link(packageId, string.Format("https://www.nuget.org/packages/{0}", packageId))));
            }
        }
        
        markdown.Add(Element.Separator);

        if (typeDocumentation.Properties?.TryGetValue("Summary", out var summary) == true)
        {
            markdown.Add(new Text(summary?.ToString()!)
            {
                IsBold = true
            });
        }

        if (typeDocumentation.Properties?.TryGetValue("Definition", out var definition) == true)
        {
            markdown.Add(new Code(definition?.ToString()!));
        }

        var inheritanceChain = BuildInheritanceChain(markdown, typeDocumentation).Reverse().ToArray();
        if (inheritanceChain.Length != 0)
        {
            var inheritanceText = new Text("Inheritance: ");
            for (var i = 0; i < inheritanceChain.Length; i++)
            {
                if (i != 0) inheritanceText.Append(" \u2192 ");
                inheritanceText.Append(inheritanceChain[i]);
            }

            inheritanceText.Append(" \u2192 ");
            inheritanceText.Append(name);

            markdown.Add(inheritanceText);
        }

        await WriteMarkdownAsync(typeDocumentationFile, markdown, cancellationToken);
    }

    private async Task BuildAssembliesTableOfContentsAsync(MarkdownBuilder markdown, TableOfContents tableOfContents,
        CancellationToken cancellationToken)
    {
        var items = new List<Element>();
        foreach (var child in tableOfContents.Children)
        {
            if (child is not TableOfContents assemblyToC) continue;

            await WriteAsync(assemblyToC, cancellationToken);

            var container = new IndentContainer(1, false);
            container.Add(new Link(assemblyToC.Id, markdown.BuildRelativePath(assemblyToC) + "/README.md"));

            if (assemblyToC.Properties?.TryGetValue("Description", out var description) == true)
            {
                container.Add(Text.Empty);
                container.Add(new Text(description?.ToString()!));
            }

            items.Add(container);
        }

        markdown.Add(new List
        {
            Items = items
        });
    }

    private static async Task WriteMarkdownAsync(FileSystemInfo fileInfo, MarkdownBuilder markdown,
        CancellationToken cancellationToken)
    {
        if (fileInfo.Exists)
        {
            var content = await File.ReadAllTextAsync(fileInfo.FullName, cancellationToken);

            var replacementRegex = new Regex(@"(\[//\]:\s*<!DOKI>).*?(\[//\]:\s*</!DOKI>)", RegexOptions.Singleline);
            var result = replacementRegex.Matches(content);
            if (result is [{Groups.Count: 3}])
            {
                content = replacementRegex.Replace(content,
                    $"{result[0].Groups[1].Value}{Environment.NewLine}{markdown}{Environment.NewLine}{result[0].Groups[2].Value}");

                await File.WriteAllTextAsync(fileInfo.FullName, content, cancellationToken);
                return;
            }
        }

        await File.WriteAllTextAsync(fileInfo.FullName, markdown.ToString(), cancellationToken);
    }

    private static Element BuildMarkdownTableOfContents(MarkdownBuilder markdown, DokiElement element, int indent)
    {
        var relativePath = markdown.BuildRelativePath(element, ".md");
        if (element is not TableOfContents toc || toc.Children.Length == 0) return new Link(element.Id, relativePath);
        return new SubList(new Link(toc.Id, relativePath), indent)
        {
            Items = toc.Children.Select(x => BuildMarkdownTableOfContents(markdown, x, indent + 1)).ToList()
        };
    }

    private static IEnumerable<Element> BuildInheritanceChain(MarkdownBuilder markdown, DokiElement element)
    {
        while (true)
        {
            if (element.Properties?.TryGetValue("BaseType", out var baseType) == true)
            {
                if (baseType is not TypeDocumentationReference typeDocumentationReference) yield break;

                if (typeDocumentationReference.Properties?.TryGetValue("IsDocumented", out var isDocumented) == true &&
                    isDocumented is true)
                {
                    yield return new Link(typeDocumentationReference.Id,
                        markdown.BuildRelativePath(typeDocumentationReference, ".md"));
                }
                else if (typeDocumentationReference.Properties?.TryGetValue("IsMicrosoft", out var isMicrosoft) ==
                         true && isMicrosoft is true)
                {
                    yield return new Link(typeDocumentationReference.Id,
                        $"https://learn.microsoft.com/en-us/dotnet/api/{typeDocumentationReference.Id}");
                }
                else
                {
                    yield return new Text(typeDocumentationReference.Id);
                }

                element = typeDocumentationReference;
                continue;
            }

            break;
        }
    }
}