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
            .Add(new Heading(tableOfContents.Name, 1))
            .Add(new List
            {
                Items = tableOfContents.Children.Select(x => BuildMarkdownTableOfContents(x, 0)).ToList()
            });

        await File.WriteAllTextAsync(tableOfContentsFile.FullName, markdown.ToString(), cancellationToken);
    }

    private static Element BuildMarkdownTableOfContents(TableOfContents toc, int indent)
    {
        if (toc.Children.Length == 0) return new Link(toc.Name, BuildPath(toc));
        return new SubList(toc.Name, indent)
        {
            Items = toc.Children.Select(x => BuildMarkdownTableOfContents(x, indent + 1)).ToList()
        };
    }

    private static string BuildPath(DokiElement element)
    {
        var path = new List<string>();

        var current = element;

        while (current.Parent != null)
        {
            path.Add(current.Name);

            current = current.Parent;
        }

        path.Reverse();

        return Path.Combine(path.ToArray());
    }
}