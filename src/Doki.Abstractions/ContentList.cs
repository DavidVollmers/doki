namespace Doki;

/// <summary>
/// Represents a list of content.
/// </summary>
public record ContentList : DocumentationObject
{
    /// <summary>
    /// Represents the assemblies content list.
    /// </summary>
    public const string Assemblies = "Packages";
 
    /// <summary>
    /// Gets the name of the content list.
    /// </summary>   
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// Gets the description of the content list.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the items in the content list.
    /// </summary>
    public DocumentationObject[] Items { get; set; } = [];
}