namespace Doki;

public sealed record GenericTypeArgumentDocumentation : TypeDocumentationReference
{
    public string? Description { get; internal init; }
    
    public bool IsGenericParameter { get; internal init; }
}