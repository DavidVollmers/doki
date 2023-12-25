namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public string? Summary { get; internal init; }
}