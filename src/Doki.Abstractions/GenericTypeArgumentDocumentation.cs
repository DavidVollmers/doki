namespace Doki;

public sealed record GenericTypeArgumentDocumentation : TypeDocumentationObject
{
    public string? Description { get; internal init; }
}