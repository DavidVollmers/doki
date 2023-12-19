using Doki.Elements;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

[DokiOutput("Doki.Output.Markdown")]
public sealed class MarkdownOutput(OutputContext context) : OutputBase<OutputOptions>(context)
{
    public override async Task WriteAsync(TableOfContents tableOfContents,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableOfContents);

        var tableOfContentsFile =
            new FileInfo(Path.Combine(OutputDirectory.FullName, BuildPath(tableOfContents), "README.md"));

        if (!tableOfContentsFile.Directory!.Exists) tableOfContentsFile.Directory.Create();

        var markdown = new MarkdownBuilder()
            .Add(new Heading(tableOfContents.Name, 1));

        switch (tableOfContents.Content)
        {
            case DokiContent.Namespaces:
                var items = new List<Element>();
                foreach (var namespaceToC in tableOfContents.Children)
                {
                    await WriteAsync(namespaceToC, cancellationToken);

                    items.Add(new Link(namespaceToC.Name, Path.Combine(BuildPath(namespaceToC), "README.md")));
                }

                markdown.Add(new List
                {
                    Items = items
                });
                break;
            case DokiContent.Namespace:
            case DokiContent.TypeReference:
            default:
                markdown.Add(new List
                {
                    Items = tableOfContents.Children.Select(x => BuildMarkdownTableOfContents(x, 0)).ToList()
                });
                break;
        }

        await File.WriteAllTextAsync(tableOfContentsFile.FullName, markdown.ToString(), cancellationToken);
    }

    private static Element BuildMarkdownTableOfContents(TableOfContents toc, int indent)
    {
        if (toc.Children.Length == 0) return new Link(toc.Name, BuildPath(toc, ".md"));
        return new SubList(new Link(toc.Name, BuildPath(toc, ".md")), indent)
        {
            Items = toc.Children.Select(x => BuildMarkdownTableOfContents(x, indent + 1)).ToList()
        };
    }

    //TODO build relative path to current path
    private static string BuildPath(DokiElement element, string? suffix = null)
    {
        var path = new List<string>();

        var current = element;

        while (current.Parent != null)
        {
            path.Add(current.Name);

            current = current.Parent;
        }

        path.Reverse();

        var result = Path.Combine(path.ToArray());
        return suffix == null ? result : $"{result}{suffix}";
    }
}