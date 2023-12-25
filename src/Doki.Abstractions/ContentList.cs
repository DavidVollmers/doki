namespace Doki;

public record ContentList : DocumentationObject
{
    public const string Assemblies = "Packages";
    
    public string Name { get; internal init; } = null!;
    
    public string? Description { get; internal init; }

    public DocumentationObject[] Items { get; internal set; } = Array.Empty<DocumentationObject>();
}