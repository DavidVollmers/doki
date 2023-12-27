namespace Doki;

public sealed record TypeDocumentation : TypeDocumentationReference
{
    public string? Summary { get; internal init; }

    public string Definition { get; internal init; } = null!;
}