namespace Doki.Output.Markdown;

public sealed record MarkdownOutputOptions : OutputOptions<MarkdownOutput>
{
    public bool PathShortening { get; init; } = true;
}