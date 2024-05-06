namespace Doki;

/// <summary>
/// Represents a link in the documentation.
/// </summary>
public sealed record Link : DocumentationObject
{
    /// <summary>
    /// Gets the URL of the link.
    /// </summary>
    public string Url { get; init; } = null!;

    /// <summary>
    /// Gets the text of the link.
    /// </summary>
    public string Text { get; init; } = null!;

    public Link()
    {
        Content = DocumentationContentType.Link;
    }
}