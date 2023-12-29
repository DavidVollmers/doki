namespace Doki;

public sealed record Link : DocumentationObject
{
    public string Url { get; init; } = null!;

    public string Text { get; init; } = null!;
}