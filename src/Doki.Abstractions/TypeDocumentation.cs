namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public string Definition { get; internal init; } = null!;

    public DocumentationObject[] Examples { get; internal set; } = Array.Empty<DocumentationObject>();
    
    public DocumentationObject[] Remarks { get; internal set; } = Array.Empty<DocumentationObject>();

    public TypeDocumentationReference[] Interfaces { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    public TypeDocumentationReference[] DerivedTypes { get; internal set; } = Array.Empty<TypeDocumentationReference>();
    
    public MemberDocumentation[] Constructors { get; internal set; } = Array.Empty<MemberDocumentation>();
    
    public MemberDocumentation[] Fields { get; internal set; } = Array.Empty<MemberDocumentation>();
    
    public MemberDocumentation[] Properties { get; internal set; } = Array.Empty<MemberDocumentation>();
    
    public MemberDocumentation[] Methods { get; internal set; } = Array.Empty<MemberDocumentation>();
}