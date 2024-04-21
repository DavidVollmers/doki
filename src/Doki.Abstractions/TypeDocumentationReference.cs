namespace Doki;

/// <summary>
/// Represents a type documentation reference in the documentation.
/// </summary>
public record TypeDocumentationReference : MemberDocumentation
{
    /// <summary>
    /// Gets a value indicating whether the type is generic.
    /// </summary>
    public bool IsGeneric { get; internal init; }

    /// <summary>
    /// Gets the full name of the type.
    /// </summary>
    public string FullName { get; internal init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the type is documented.
    /// </summary>
    public bool IsDocumented { get; internal init; }

    /// <summary>
    /// Gets a value indicating whether the type is from Microsoft.
    /// </summary>
    public bool IsMicrosoft { get; internal init; }

    /// <summary>
    /// Gets the base type of the type.
    /// </summary>
    public TypeDocumentationReference? BaseType { get; internal set; }

    /// <summary>
    /// Gets the generic arguments of the type.
    /// </summary>
    public GenericTypeArgumentDocumentation[] GenericArguments { get; internal set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDocumentationReference"/> class.
    /// </summary>
    public TypeDocumentationReference()
    {
        Content = DocumentationContentType.TypeReference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDocumentationReference"/> class.
    /// </summary>
    /// <param name="reference">The reference to copy.</param>
    public TypeDocumentationReference(TypeDocumentationReference reference) : base(reference)
    {
        Content = DocumentationContentType.TypeReference;
        IsGeneric = reference.IsGeneric;
        GenericArguments = reference.GenericArguments;
        FullName = reference.FullName;
        IsDocumented = reference.IsDocumented;
        IsMicrosoft = reference.IsMicrosoft;
        BaseType = reference.BaseType;
    }
}