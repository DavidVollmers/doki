﻿using System.Text.RegularExpressions;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

public sealed class MarkdownOutput(OutputOptions<MarkdownOutput> options) : IOutput
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

        markdown.Add(new Heading("Namespaces", 2));
        markdown.Add(new List
        {
            Items = assemblyDocumentation.Namespaces.Select(x => markdown.BuildLinkTo(x)).ToList()
        });

        await WriteMarkdownAsync(file, markdown, cancellationToken);
    }

    public async Task WriteAsync(NamespaceDocumentation namespaceDocumentation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(namespaceDocumentation);

        var (file, markdown) = Prepare(namespaceDocumentation, namespaceDocumentation.Name,
            namespaceDocumentation.Description);

        markdown.Add(new Heading("Types", 2));
        markdown.Add(new List
        {
            Items = namespaceDocumentation.Types.Select(x => markdown.BuildLinkTo(x)).ToList()
        });

        await WriteMarkdownAsync(file, markdown, cancellationToken);
    }

    public async Task WriteAsync(TypeDocumentation typeDocumentation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(typeDocumentation);

        var currentPath = typeDocumentation.GetPath() + ".md";

        var typeDocumentationFile = new FileInfo(Path.Combine(options.OutputDirectory.FullName, currentPath));

        if (!typeDocumentationFile.Directory!.Exists) typeDocumentationFile.Directory.Create();

        var markdown = new MarkdownBuilder(currentPath);
        markdown.Add(markdown.BuildBreadcrumbs(typeDocumentation))
            .Add(new Heading(typeDocumentation.Name, 1).Append($" {Enum.GetName(typeDocumentation.ContentType)}"))
            .Add(new Heading(nameof(TypeDocumentation.Definition), 2));

        var namespaceDocumentation =
            typeDocumentation.TryGetParent<NamespaceDocumentation>(DocumentationContentType.Namespace);
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

        if (typeDocumentation.Summaries.Length != 0)
        {
            foreach (var summary in typeDocumentation.Summaries)
            {
                var text = markdown.BuildText(summary);
                text.IsBold = true;
                markdown.Add(text);
            }
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
                var text = Text.Empty;
                foreach (var summary in constructor.Summaries)
                {
                    text.Append(markdown.BuildText(summary));
                    text.NewLine();
                }

                table.AddRow(new Text(constructor.Name), text);
            }

            markdown.Add(table);
        }

        if (typeDocumentation.Fields.Length != 0)
        {
            markdown.Add(new Heading("Fields", 2));

            var table = new Table(new Text("   "), new Text("Summary"));
            foreach (var field in typeDocumentation.Fields)
            {
                var text = Text.Empty;
                foreach (var summary in field.Summaries)
                {
                    text.Append(markdown.BuildText(summary));
                    text.NewLine();
                }
                
                table.AddRow(new Text(field.Name), text);
            }

            markdown.Add(table);
        }

        if (typeDocumentation.Properties.Length != 0)
        {
            markdown.Add(new Heading("Properties", 2));

            var table = new Table(new Text("   "), new Text("Summary"));
            foreach (var property in typeDocumentation.Properties)
            {
                var text = Text.Empty;
                foreach (var summary in property.Summaries)
                {
                    text.Append(markdown.BuildText(summary));
                    text.NewLine();
                }

                table.AddRow(new Text(property.Name), text);
            }

            markdown.Add(table);
        }

        if (typeDocumentation.Methods.Length != 0)
        {
            markdown.Add(new Heading("Methods", 2));

            var table = new Table(new Text("   "), new Text("Summary"));
            foreach (var method in typeDocumentation.Methods)
            {
                var text = Text.Empty;
                foreach (var summary in method.Summaries)
                {
                    text.Append(markdown.BuildText(summary));
                    text.NewLine();
                }

                table.AddRow(new Text(method.Name), text);
            }

            markdown.Add(table);
        }

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
        var currentPath = documentationObject.GetPath();

        var file = new FileInfo(Path.Combine(options.OutputDirectory.FullName, currentPath, "README.md"));

        if (!file.Directory!.Exists) file.Directory.Create();

        var heading = new Heading(name, 1);
        if (documentationObject.ContentType == DocumentationContentType.Namespace) heading.Append(" Namespace");

        var markdown = new MarkdownBuilder(documentationObject.GetPath()).Add(heading);

        if (description != null)
        {
            markdown.Add(new Text(description));
        }

        return (file, markdown);
    }
}