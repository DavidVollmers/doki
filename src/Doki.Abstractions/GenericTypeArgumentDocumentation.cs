namespace Doki;

public sealed record GenericTypeArgumentDocumentation : TypeDocumentationReference
{
    public DocumentationObject? Description { get; internal set; }

    public bool IsGenericParameter { get; internal init; }
}