using System.Text;
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
            default:
                if (tableOfContents.Content == DokiContent.Assembly)
                {
                    foreach (var namespaceToC in tableOfContents.Children)
                    {
                        await WriteAsync(namespaceToC, cancellationToken);
                    }
                    
                    markdown.Add(new Heading("Namespaces", 2));
                }

                markdown.Add(new List
                {
                    Items = tableOfContents.Children.Select(x => BuildMarkdownTableOfContents(markdown, x, 0)).ToList()
                });
                break;
        }

        await WriteMarkdownAsync(tableOfContentsFile, markdown, cancellationToken);
    }

    private async Task BuildAssembliesTableOfContentsAsync(MarkdownBuilder markdown, TableOfContents tableOfContents,
        CancellationToken cancellationToken)
    {
        var items = new List<Element>();
        foreach (var assemblyToC in tableOfContents.Children)
        {
            await WriteAsync(assemblyToC, cancellationToken);

            var container = new IndentContainer(1, false);
            container.Add(new Link(assemblyToC.Id, Path.Combine(markdown.BuildRelativePath(assemblyToC), "README.md")));

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

            var replacementRegex = ReplacementMarkRegex();
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

    private static Element BuildMarkdownTableOfContents(MarkdownBuilder markdown, TableOfContents toc, int indent)
    {
        var relativePath = markdown.BuildRelativePath(toc, ".md");
        if (toc.Children.Length == 0) return new Link(toc.Id, relativePath);
        return new SubList(new Link(toc.Id, relativePath), indent)
        {
            Items = toc.Children.Select(x => BuildMarkdownTableOfContents(markdown, x, indent + 1)).ToList()
        };
    }

    [GeneratedRegex(@"(\[//\]:\s*<!DOKI>).*?(\[//\]:\s*</!DOKI>)", RegexOptions.Singleline)]
    private static partial Regex ReplacementMarkRegex();
}