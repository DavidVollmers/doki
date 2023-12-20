namespace Doki;

public sealed record TableOfContents : DokiElement
{
    public const string Assemblies = "Packages";
    
    public TableOfContents[] Children { get; set; } = Array.Empty<TableOfContents>();
}