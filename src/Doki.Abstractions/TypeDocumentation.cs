namespace Doki;

/// <summary>
/// Represents a type documentation.
/// </summary>
public sealed record TypeDocumentation : TypeDocumentationReference
{
    /// <summary>
    /// Gets the definition of the type.
    /// </summary>
    public string Definition { get; init; } = null!;

    /// <summary>
    /// Get the examples of the type.
    /// </summary>
    public XmlDocumentation[] Examples { get; init; } = [];

    /// <summary>
    /// Gets the remarks of the type.
    /// </summary>
    public XmlDocumentation[] Remarks { get; init; } = [];

    /// <summary>
    /// Gets the interfaces implemented by the type.
    /// </summary>
    public TypeDocumentationReference[] Interfaces { get; init; } = [];

    /// <summary>
    /// Gets the derived types of the type.
    /// </summary>
    public TypeDocumentationReference[] DerivedTypes { get; init; } = [];

    /// <summary>
    /// Gets the constructors of the type.
    /// </summary>
    public MemberDocumentation[] Constructors { get; init; } = [];

    /// <summary>
    /// Gets the fields of the type.
    /// </summary>
    public MemberDocumentation[] Fields { get; init; } = [];

    /// <summary>
    /// Gets the properties of the type.
    /// </summary>
    public MemberDocumentation[] Properties { get; init; } = [];

    /// <summary>
    /// Gets the methods of the type.
    /// </summary>
    public MemberDocumentation[] Methods { get; init; } = [];
    
    public new DocumentationContentType ContentType { get; init; }
}