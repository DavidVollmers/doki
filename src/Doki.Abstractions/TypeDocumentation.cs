namespace Doki;

/// <summary>
/// Represents a type documentation.
/// </summary>
public sealed record TypeDocumentation : TypeDocumentationReference
{
    /// <summary>
    /// Gets the definition of the type.
    /// </summary>
    public string Definition { get; internal init; } = null!;

    /// <summary>
    /// Get the examples of the type.
    /// </summary>
    public DocumentationObject[] Examples { get; internal set; } = Array.Empty<DocumentationObject>();

    /// <summary>
    /// Gets the remarks of the type.
    /// </summary>
    public DocumentationObject[] Remarks { get; internal set; } = Array.Empty<DocumentationObject>();

    /// <summary>
    /// Gets the interfaces implemented by the type.
    /// </summary>
    public TypeDocumentationReference[] Interfaces { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    /// <summary>
    /// Gets the derived types of the type.
    /// </summary>
    public TypeDocumentationReference[] DerivedTypes { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    /// <summary>
    /// Gets the constructors of the type.
    /// </summary>
    public MemberDocumentation[] Constructors { get; internal set; } = Array.Empty<MemberDocumentation>();

    /// <summary>
    /// Gets the fields of the type.
    /// </summary>
    public MemberDocumentation[] Fields { get; internal set; } = Array.Empty<MemberDocumentation>();

    /// <summary>
    /// Gets the properties of the type.
    /// </summary>
    public MemberDocumentation[] Properties { get; internal set; } = Array.Empty<MemberDocumentation>();

    /// <summary>
    /// Gets the methods of the type.
    /// </summary>
    public MemberDocumentation[] Methods { get; internal set; } = Array.Empty<MemberDocumentation>();
}