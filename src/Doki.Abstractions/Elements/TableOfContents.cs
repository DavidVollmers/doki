namespace Doki.Elements;

public sealed record TableOfContents : DokiElement
{
    public TableOfContents[] Children { get; set; } = Array.Empty<TableOfContents>();
}