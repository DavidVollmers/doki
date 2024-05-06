namespace Doki;

/// <summary>
/// Represents the documentation for a member.
/// </summary>
public record MemberDocumentation : DocumentationObject
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; internal init; } = null!;

    /// <summary>
    /// Gets the namespace of the member.
    /// </summary>
    public string? Namespace { get; internal init; }

    /// <summary>
    /// Gets the assembly of the member.
    /// </summary>
    public string? Assembly { get; internal init; }
    
    /// <summary>
    /// Gets the summary of the member.
    /// </summary>
    public DocumentationObject? Summary { get; internal set; }
    
    public new DocumentationContentType Content { get; internal init; }
}