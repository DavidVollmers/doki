using System.Text.RegularExpressions;
using Doki.Extensions;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

public sealed class MarkdownOutput(MarkdownOutputOptions options) : IOutput
{
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        options.ClearOutputDirectoryIfRequired();

        return Task.CompletedTask;
    }

    public async Task WriteAsync(DocumentationRoot root, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(root);

        var (file, markdown) = Prepare(root, "Packages");

        var items = new List<Element>();
        foreach (var assemblyDocumentation in root.Assemblies)
        {
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

        await WriteMarkdownAsync(file, markdown, cancellationToken);
    }

    public async Task WriteAsync(AssemblyDocumentation assemblyDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(assemblyDocumentation);

        var (file, markdown) = Prepare(assemblyDocumentation, assemblyDocumentation.Name,
            assemblyDocumentation.Description);

        markdown.AddHeadingWithList("Namespaces", assemblyDocumentation.Namespaces);

        await WriteMarkdownAsync(file, markdown, cancellationToken);
    }

    public async Task WriteAsync(NamespaceDocumentation namespaceDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(namespaceDocumentation);

        var (file, markdown) = Prepare(namespaceDocumentation, namespaceDocumentation.Name,
            namespaceDocumentation.Description);

        markdown.AddHeadingWithList("Types", namespaceDocumentation.Types);

        await WriteMarkdownAsync(file, markdown, cancellationToken);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);

        var currentPath = typeDocumentation.GetPath(options.PathShortening);

        var typeDocumentationFile =
            new FileInfo(Path.Combine(options.OutputDirectory.FullName, currentPath, "README.md"));

        if (!typeDocumentationFile.Directory!.Exists) typeDocumentationFile.Directory.Create();

        var markdown = new MarkdownBuilder(currentPath, options);
        markdown.Add(markdown.BuildBreadcrumbs(typeDocumentation))
            .Add(new Heading(typeDocumentation.Name, 1).Append($" {Enum.GetName(typeDocumentation.ContentType)}"))
            .Add(new Heading(nameof(TypeDocumentation.Definition), 2));

        var namespaceDocumentation = typeDocumentation.TryGetByParents<NamespaceDocumentation>();
        if (namespaceDocumentation != null)
        {
            markdown.Add(new Text("Namespace: ").Append(markdown.BuildLinkTo(namespaceDocumentation)));
        }

        var assemblyAssemblyDocumentation = typeDocumentation.TryGetByParents<AssemblyDocumentation>();
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

        markdown.AddBoldXmlDocumentation(typeDocumentation.Summaries);

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

        markdown.AddTextWithTypeDocumentationReferences("Derived: ", typeDocumentation.DerivedTypes);

        markdown.AddTextWithTypeDocumentationReferences("Implements: ", typeDocumentation.Interfaces);

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

        markdown.AddHeadingWithXmlDocumentation("Examples", typeDocumentation.Examples);

        markdown.AddHeadingWithXmlDocumentation("Remarks", typeDocumentation.Remarks);

        markdown.AddHeadingWithMemberTable("Constructors", typeDocumentation.Constructors);

        markdown.AddHeadingWithMemberTable("Fields", typeDocumentation.Fields);

        markdown.AddHeadingWithMemberTable("Properties", typeDocumentation.Properties);

        markdown.AddHeadingWithMemberTable("Methods", typeDocumentation.Methods);

        await WriteMarkdownAsync(typeDocumentationFile, markdown, cancellationToken);
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

    private static IEnumerable<Element> BuildInheritanceChain(MarkdownBuilder markdown,
        TypeDocumentationReference typeDocumentationReference)
    {
        while (typeDocumentationReference.BaseType != null)
        {
            yield return markdown.BuildLinkTo(typeDocumentationReference.BaseType);

            typeDocumentationReference = typeDocumentationReference.BaseType;
        }
    }

    private (FileInfo, MarkdownBuilder) Prepare(DocumentationObject documentationObject, string name,
        string? description = null)
    {
        var currentPath = documentationObject.GetPath(options.PathShortening);

        var file = new FileInfo(Path.Combine(options.OutputDirectory.FullName, currentPath, "README.md"));

        if (!file.Directory!.Exists) file.Directory.Create();

        var heading = new Heading(name, 1);
        if (documentationObject.ContentType == DocumentationContentType.Namespace) heading.Append(" Namespace");

        var markdown = new MarkdownBuilder(currentPath, options).Add(heading);

        if (description != null)
        {
            markdown.Add(new Text(description));
        }

        return (file, markdown);
    }
}