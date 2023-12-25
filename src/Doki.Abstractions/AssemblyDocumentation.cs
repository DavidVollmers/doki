namespace Doki;

public sealed record AssemblyDocumentation : ContentList
{
    public string FileName { get; internal init; } = null!;

    public string? Version { get; internal init; }
    
    public string? PackageId { get; internal init; }
}