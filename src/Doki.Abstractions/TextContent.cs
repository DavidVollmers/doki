namespace Doki;

/// <summary>
/// Represents a text content in the documentation.
/// </summary>
public sealed record TextContent : DocumentationObject
{
    /// <summary>
    /// Gets the text content.
    /// </summary>
    public string Text { get; internal init; } = null!;
}