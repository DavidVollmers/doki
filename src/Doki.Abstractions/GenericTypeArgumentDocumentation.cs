namespace Doki;

/// <summary>
/// Represents a generic type argument in the documentation.
/// </summary>
public sealed record GenericTypeArgumentDocumentation : TypeDocumentationReference
{
    /// <summary>
    /// Gets the description of the generic type argument.
    /// </summary>
    public DocumentationObject? Description { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether the generic type argument is a generic parameter.
    /// </summary>
    public bool IsGenericParameter { get; internal init; }
}