namespace Doki;

/// <summary>
/// Represents the documentation for an assembly.
/// </summary>
public sealed record AssemblyDocumentation : ContentList
{
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    public string FileName { get; init; } = null!;

    /// <summary>
    /// Gets the version of the assembly.
    /// </summary>
    public string? Version { get; init; }
    
    /// <summary>
    /// Gets the NuGet package ID of the assembly.
    /// </summary>
    public string? PackageId { get; init; }
}