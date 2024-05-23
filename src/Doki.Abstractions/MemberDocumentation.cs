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

    /// <summary>
    /// Gets a value indicating whether the type is documented.
    /// </summary>
    public bool IsDocumented { get; init; }

    public new DocumentationContentType ContentType { get; init; }
}