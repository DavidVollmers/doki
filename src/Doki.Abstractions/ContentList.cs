namespace Doki;

public record ContentList : DokiElement
{
    public const string Assemblies = "Packages";

    public DokiElement[] Items { get; set; } = Array.Empty<DokiElement>();
}