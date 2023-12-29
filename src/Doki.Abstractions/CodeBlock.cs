namespace Doki;

public sealed record CodeBlock : DocumentationObject
{
    public string? Language { get; internal init; }

    public string Code { get; internal init; } = null!;
}