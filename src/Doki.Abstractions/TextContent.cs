namespace Doki;

public sealed record TextContent : DocumentationObject
{
    public string Text { get; internal init; } = null!;
}