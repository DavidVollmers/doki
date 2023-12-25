using System.Text.RegularExpressions;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

[DokiOutput("Doki.Output.Markdown")]
public sealed class MarkdownOutput(OutputContext context) : OutputBase<OutputOptions>(context)
{
    public override async Task WriteAsync(ContentList contentList,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contentList);

        var currentPath = contentList.GetPath();

        var targetFile = new FileInfo(Path.Combine(OutputDirectory.FullName, currentPath, "README.md"));

        if (!targetFile.Directory!.Exists) targetFile.Directory.Create();

        var markdown = new MarkdownBuilder(currentPath)
            .Add(new Heading(contentList.Name, 1));

        if (contentList.Description != null)
        {
            markdown.Add(new Text(contentList.Description));
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (contentList.Content)
        {
            case DokiContent.Assemblies:
                await BuildAssembliesListAsync(markdown, contentList, cancellationToken);
                break;
            case DokiContent.Assembly:
                foreach (var item in contentList.Items)
                {
                    if (item is not ContentList namespaceDocumentation) continue;

                    await WriteAsync(namespaceDocumentation, cancellationToken);
                }

                markdown.Add(new Heading("Namespaces", 2));
                markdown.Add(new List
                {
                    Items = contentList.Items.Select(x => (Element)markdown.BuildLinkTo(x)).ToList()
                });
                break;
            case DokiContent.Namespace:
                markdown.Add(new Heading("Types", 2));
                goto default;
            default:
                markdown.Add(new List
                {
                    Items = contentList.Items.Select(x => BuildMarkdownList(markdown, x, 0)).ToList()
                });
                break;
        }

        await WriteMarkdownAsync(targetFile, markdown, cancellationToken);
    }

    public override async Task WriteAsync(TypeDocumentation typeDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);

        var currentPath = typeDocumentation.GetPath() + ".md";

        var typeDocumentationFile = new FileInfo(Path.Combine(OutputDirectory.FullName, currentPath));

        if (!typeDocumentationFile.Directory!.Exists) typeDocumentationFile.Directory.Create();

        var name = typeDocumentation.Properties?.TryGetValue(DokiProperties.Name, out var nameProperty) == true
            ? nameProperty?.ToString()!
            : typeDocumentation.Id;

        var markdown = new MarkdownBuilder(currentPath)
            .Add(new Heading(name, 1).Append($" {Enum.GetName(typeDocumentation.Content)}"))
            .Add(new Heading(DokiProperties.Definition, 2));

        var namespaceDocumentation = typeDocumentation.TryGetParent<ContentList>(DokiContent.Namespace);
        if (namespaceDocumentation != null)
        {
            markdown.Add(new Text("Namespace: ").Append(markdown.BuildLinkTo(namespaceDocumentation)));
        }

        var assemblyAssemblyDocumentation = typeDocumentation.TryGetParent<AssemblyDocumentation>();
        if (assemblyAssemblyDocumentation != null)
        {
            markdown.Add(new Text("Assembly: ").Append(markdown.BuildLinkTo(assemblyAssemblyDocumentation,
                DokiProperties.FileName)));

            var packageId =
                assemblyAssemblyDocumentation.Properties?.TryGetValue(DokiProperties.PackageId,
                    out var packageIdProperty) == true
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

        if (typeDocumentation.Properties?.TryGetValue(DokiProperties.Summary, out var summary) == true)
        {
            markdown.Add(new Text(summary?.ToString()!)
            {
                IsBold = true
            });
        }

        if (typeDocumentation.Properties?.TryGetValue(DokiProperties.Definition, out var definition) == true)
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

    private async Task BuildAssembliesListAsync(MarkdownBuilder markdown, ContentList contentList,
        CancellationToken cancellationToken)
    {
        var items = new List<Element>();
        foreach (var item in contentList.Items)
        {
            if (item is not AssemblyDocumentation assemblyDocumentation) continue;

            await WriteAsync(assemblyDocumentation, cancellationToken);

            var container = new IndentContainer(1, false);
            container.Add(markdown.BuildLinkTo(assemblyDocumentation));

            if (assemblyDocumentation.Properties?.TryGetValue(DokiProperties.Description, out var description) == true)
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
            if (result is [{ Groups.Count: 3 }])
            {
                content = replacementRegex.Replace(content,
                    $"{result[0].Groups[1].Value}{Environment.NewLine}{markdown}{Environment.NewLine}{result[0].Groups[2].Value}");

                await File.WriteAllTextAsync(fileInfo.FullName, content, cancellationToken);
                return;
            }
        }

        await File.WriteAllTextAsync(fileInfo.FullName, markdown.ToString(), cancellationToken);
    }

    private static Element BuildMarkdownList(MarkdownBuilder markdown, DokiElement element, int indent)
    {
        var link = markdown.BuildLinkTo(element);
        if (element is not ContentList contentList || contentList.Items.Length == 0) return link;
        return new SubList(link, indent)
        {
            Items = contentList.Items.Select(x => BuildMarkdownList(markdown, x, indent + 1)).ToList()
        };
    }

    private static IEnumerable<Element> BuildInheritanceChain(MarkdownBuilder markdown, DokiElement element)
    {
        while (true)
        {
            if (element.Properties?.TryGetValue("BaseType", out var baseType) == true)
            {
                if (baseType is not TypeDocumentationReference typeDocumentationReference) yield break;

                yield return markdown.BuildLinkTo(typeDocumentationReference);

                element = typeDocumentationReference;
                continue;
            }

            break;
        }
    }
}