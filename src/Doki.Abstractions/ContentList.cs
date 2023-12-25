namespace Doki;

public record ContentList : DokiElement
{
    public const string Assemblies = "Packages";
    
    public string Name { get; internal init; } = null!;
    
    public string? Description { get; internal init; }

    public DokiElement[] Items { get; internal set; } = Array.Empty<DokiElement>();
}