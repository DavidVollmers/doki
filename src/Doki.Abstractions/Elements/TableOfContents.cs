namespace Doki.Elements;

public sealed record TableOfContents : DokiElement
{
    public const string Namespaces = "Namespaces";
    
    public TableOfContents[] Children { get; set; } = Array.Empty<TableOfContents>();
}