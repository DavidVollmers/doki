namespace Doki;

/// <summary>
/// Represents a code block in the documentation.
/// </summary>
public sealed record CodeBlock : DocumentationObject
{
    /// <summary>
    /// Gets the language of the code block.
    /// </summary>
    public string? Language { get; init; }

    /// <summary>
    /// Gets the code of the block.
    /// </summary>
    public string Code { get; init; } = null!;

    public CodeBlock()
    {
        ContentType = DocumentationContentType.CodeBlock;
    }
}