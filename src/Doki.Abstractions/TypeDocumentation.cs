namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public DocumentationObject? Summary { get; internal set; }

    public string Definition { get; internal init; } = null!;

    public DocumentationObject[] Examples { get; internal set; } = Array.Empty<DocumentationObject>();
    
    public DocumentationObject[] Remarks { get; internal set; } = Array.Empty<DocumentationObject>();

    public TypeDocumentationReference[] Interfaces { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    public TypeDocumentationReference[] DerivedTypes { get; internal set; } = Array.Empty<TypeDocumentationReference>();
    
    public ConstructorDocumentation[] Constructors { get; internal set; } = Array.Empty<ConstructorDocumentation>();
}