namespace Doki;

/// <summary>
/// Represents the documentation for a member.
/// </summary>
public record MemberDocumentation : DocumentationObject
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the namespace of the member.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the assembly of the member.
    /// </summary>
    public string? Assembly { get; init; }

    /// <summary>
    /// Gets the summary of the member.
    /// </summary>
    public XmlDocumentation? Summary { get; set; }

    public new DocumentationContentType ContentType { get; init; }
}