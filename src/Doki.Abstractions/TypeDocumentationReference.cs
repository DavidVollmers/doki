namespace Doki;

/// <summary>
/// Represents a type documentation reference in the documentation.
/// </summary>
public record TypeDocumentationReference : MemberDocumentation
{
    /// <summary>
    /// Gets a value indicating whether the type is generic.
    /// </summary>
    public bool IsGeneric { get; init; }

    /// <summary>
    /// Gets the full name of the type.
    /// </summary>
    public string FullName { get; init; } = null!;

    /// <summary>
    /// Gets a value indicating whether the type is documented.
    /// </summary>
    public bool IsDocumented { get; init; }

    /// <summary>
    /// Gets a value indicating whether the type is from Microsoft.
    /// </summary>
    public bool IsMicrosoft { get; init; }

    internal TypeDocumentationReference? InternalBaseType;
    
    /// <summary>
    /// Gets the base type of the type.
    /// </summary>
    public TypeDocumentationReference? BaseType
    {
        get => InternalBaseType;
        init => InternalBaseType = value;
    }

    internal GenericTypeArgumentDocumentation[] InternalGenericArguments = [];

    /// <summary>
    /// Gets the generic arguments of the type.
    /// </summary>
    public GenericTypeArgumentDocumentation[] GenericArguments
    {
        get => InternalGenericArguments;
        init => InternalGenericArguments = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeDocumentationReference"/> class.
    /// </summary>
    public TypeDocumentationReference()
    {
        ContentType = DocumentationContentType.TypeReference;
    }
}