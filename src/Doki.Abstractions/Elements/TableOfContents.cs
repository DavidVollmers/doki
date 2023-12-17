namespace Doki.Elements;

public sealed record TableOfContents : DokiElement
{
    public TableOfContents[] Children { get; init; } = Array.Empty<TableOfContents>();
}