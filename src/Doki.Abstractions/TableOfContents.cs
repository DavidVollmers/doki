namespace Doki;

public sealed record TableOfContents : DokiElement
{
    public const string Assemblies = "Packages";

    public DokiElement[] Children { get; set; } = Array.Empty<DokiElement>();
}