namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public string? Summary { get; internal init; }

    public string Definition { get; internal init; } = null!;

    public TypeDocumentationReference[] Interfaces { get; internal set; } = Array.Empty<TypeDocumentationReference>();

    public TypeDocumentationReference[] DerivedTypes { get; internal set; } = Array.Empty<TypeDocumentationReference>();
}