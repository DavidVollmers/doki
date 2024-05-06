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

    internal XmlDocumentation[] InternalExamples = [];

    /// <summary>
    /// Get the examples of the type.
    /// </summary>
    public XmlDocumentation[] Examples
    {
        get => InternalExamples;
        init => InternalExamples = value;
    }

    internal XmlDocumentation[] InternalRemarks = [];

    /// <summary>
    /// Gets the remarks of the type.
    /// </summary>
    public XmlDocumentation[] Remarks
    {
        get => InternalRemarks;
        init => InternalRemarks = value;
    }

    internal TypeDocumentationReference[] InternalInterfaces = [];

    /// <summary>
    /// Gets the interfaces implemented by the type.
    /// </summary>
    public TypeDocumentationReference[] Interfaces
    {
        get => InternalInterfaces;
        init => InternalInterfaces = value;
    }

    internal TypeDocumentationReference[] InternalDerivedTypes = [];

    /// <summary>
    /// Gets the derived types of the type.
    /// </summary>
    public TypeDocumentationReference[] DerivedTypes
    {
        get => InternalDerivedTypes;
        init => InternalDerivedTypes = value;
    }

    internal MemberDocumentation[] InternalConstructors = [];
    
    /// <summary>
    /// Gets the constructors of the type.
    /// </summary>
    public MemberDocumentation[] Constructors
    {
        get => InternalConstructors;
        init => InternalConstructors = value;
    }
    
    internal MemberDocumentation[] InternalFields = [];

    /// <summary>
    /// Gets the fields of the type.
    /// </summary>
    public MemberDocumentation[] Fields
    {
        get => InternalFields;
        init => InternalFields = value;
    }
    
    internal MemberDocumentation[] InternalProperties = [];

    /// <summary>
    /// Gets the properties of the type.
    /// </summary>
    public MemberDocumentation[] Properties
    {
        get => InternalProperties;
        init => InternalProperties = value;
    }
    
    internal MemberDocumentation[] InternalMethods = [];

    /// <summary>
    /// Gets the methods of the type.
    /// </summary>
    public MemberDocumentation[] Methods
    {
        get => InternalMethods;
        init => InternalMethods = value;
    }

    public new DocumentationContentType ContentType { get; init; }
}