namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public DocumentationObject? Summary { get; internal set; }

    public string Definition { get; internal init; } = null!;

    public ExampleDocumentation[] Examples { get; internal set; } = Array.Empty<ExampleDocumentation>();

    public TypeDocumentationReference[] Interfaces { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    public TypeDocumentationReference[] DerivedTypes { get; internal set; } = Array.Empty<TypeDocumentationReference>();
}