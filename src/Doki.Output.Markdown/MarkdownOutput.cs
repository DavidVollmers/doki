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

        var heading = new Heading(contentList.Name, 1);
        if (contentList.Content == DocumentationContent.Namespace) heading.Append(" Namespace");

        var markdown = new MarkdownBuilder(currentPath).Add(heading);

        if (contentList.Description != null)
        {
            markdown.Add(new Text(contentList.Description));
        }

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (contentList.Content)
        {
            case DocumentationContent.Assemblies:
                await BuildAssembliesListAsync(markdown, contentList, cancellationToken);
                break;
            case DocumentationContent.Assembly:
                foreach (var item in contentList.Items)
                {
                    if (item is not ContentList namespaceDocumentation) continue;

                    await WriteAsync(namespaceDocumentation, cancellationToken);
                }

                markdown.Add(new Heading("Namespaces", 2));
                markdown.Add(new List
                {
                    Items = contentList.Items.Select(x => markdown.BuildLinkTo(x)).ToList()
                });
                break;
            case DocumentationContent.Namespace:
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

        var markdown = new MarkdownBuilder(currentPath);
        markdown.Add(markdown.BuildBreadcrumbs(typeDocumentation))
            .Add(new Heading(typeDocumentation.Name, 1).Append($" {Enum.GetName(typeDocumentation.Content)}"))
            .Add(new Heading(nameof(TypeDocumentation.Definition), 2));

        var namespaceDocumentation = typeDocumentation.TryGetParent<ContentList>(DocumentationContent.Namespace);
        if (namespaceDocumentation != null)
        {
            markdown.Add(new Text("Namespace: ").Append(markdown.BuildLinkTo(namespaceDocumentation)));
        }

        var assemblyAssemblyDocumentation = typeDocumentation.TryGetParent<AssemblyDocumentation>();
        if (assemblyAssemblyDocumentation != null)
        {
            markdown.Add(new Text("Assembly: ").Append(markdown.BuildLinkTo(assemblyAssemblyDocumentation,
                assemblyAssemblyDocumentation.FileName)));

            if (assemblyAssemblyDocumentation.PackageId != null)
            {
                markdown.Add(new Text("Package: ")
                    //TODO support other package sources
                    .Append(new Elements.Link(assemblyAssemblyDocumentation.PackageId,
                        // ReSharper disable once UseStringInterpolation
                        string.Format("https://www.nuget.org/packages/{0}", assemblyAssemblyDocumentation.PackageId))));
            }
        }

        markdown.Add(Element.Separator);

        if (typeDocumentation.Summary != null)
        {
            var summary = markdown.BuildText(typeDocumentation.Summary);
            summary.IsBold = true;
            markdown.Add(summary);
        }

        markdown.Add(new Code(typeDocumentation.Definition));

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
            inheritanceText.Append(typeDocumentation.Name);

            markdown.Add(inheritanceText);
        }

        if (typeDocumentation.DerivedTypes.Length != 0)
        {
            var derivedTypesText = new Text("Derived: ");
            for (var i = 0; i < typeDocumentation.DerivedTypes.Length; i++)
            {
                if (i != 0) derivedTypesText.Append(", ");
                derivedTypesText.Append(markdown.BuildLinkTo(typeDocumentation.DerivedTypes[i]));
            }

            markdown.Add(derivedTypesText);
        }

        if (typeDocumentation.Interfaces.Length != 0)
        {
            var interfacesText = new Text("Implements: ");
            for (var i = 0; i < typeDocumentation.Interfaces.Length; i++)
            {
                if (i != 0) interfacesText.Append(", ");
                interfacesText.Append(markdown.BuildLinkTo(typeDocumentation.Interfaces[i]));
            }

            markdown.Add(interfacesText);
        }

        if (typeDocumentation.IsGeneric)
        {
            markdown.Add(new Heading("Type Parameters", 2));
            markdown.Add(new List
            {
                Items = typeDocumentation.GenericArguments.Select(x =>
                {
                    if (x.Description == null) return (Element)new Code(x.Name, true);

                    var container = new IndentContainer(1, false);
                    container.Add(new Code(x.Name, true));
                    container.Add(Text.Empty);
                    container.Add(markdown.BuildText(x.Description));
                    return container;
                }).ToList()
            });
        }

        if (typeDocumentation.Examples.Length != 0)
        {
            markdown.Add(new Heading("Examples", 2));

            foreach (var example in typeDocumentation.Examples)
            {
                markdown.Add(markdown.BuildText(example));
            }
        }

        if (typeDocumentation.Remarks.Length != 0)
        {
            markdown.Add(new Heading("Remarks", 2));

            foreach (var remark in typeDocumentation.Remarks)
            {
                markdown.Add(markdown.BuildText(remark));
            }
        }

        if (typeDocumentation.Constructors.Length != 0)
        {
            markdown.Add(new Heading("Constructors", 2));

            var table = new Table(new Text("   "), new Text("Summary"));
            foreach (var constructor in typeDocumentation.Constructors)
            {
                table.AddRow(new Text(constructor.Name),
                    constructor.Summary == null ? Text.Empty : markdown.BuildText(constructor.Summary));
            }

            markdown.Add(table);
        }
        
        if (typeDocumentation.Fields.Length != 0)
        {
            markdown.Add(new Heading("Fields", 2));

            var table = new Table(new Text("   "), new Text("Summary"));
            foreach (var field in typeDocumentation.Fields)
            {
                table.AddRow(new Text(field.Name),
                    field.Summary == null ? Text.Empty : markdown.BuildText(field.Summary));
            }

            markdown.Add(table);
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

            if (assemblyDocumentation.Description != null)
            {
                container.Add(Text.Empty);
                container.Add(new Text(assemblyDocumentation.Description));
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

    private static Element BuildMarkdownList(MarkdownBuilder markdown, DocumentationObject element, int indent)
    {
        var link = markdown.BuildLinkTo(element);
        if (element is not ContentList contentList || contentList.Items.Length == 0) return link;
        return new SubList(link, indent)
        {
            Items = contentList.Items.Select(x => BuildMarkdownList(markdown, x, indent + 1)).ToList()
        };
    }

    private static IEnumerable<Element> BuildInheritanceChain(MarkdownBuilder markdown,
        TypeDocumentationReference typeDocumentationReference)
    {
        while (typeDocumentationReference.BaseType != null)
        {
            yield return markdown.BuildLinkTo(typeDocumentationReference.BaseType);

            typeDocumentationReference = typeDocumentationReference.BaseType;
        }
    }
}