using System.Text.Json.Serialization;

namespace Doki;

/// <summary>
/// Represents a documentation object.
/// </summary>
public abstract record DocumentationObject
{
    /// <summary>
    /// Gets the ID of the documentation object.
    /// </summary>
    public string Id { get; internal init; } = null!;

    /// <summary>
    /// Gets the parent of the documentation object.
    /// </summary>
    [JsonIgnore]
    public DocumentationObject? Parent { get; internal init; }

    /// <summary>
    /// Gets the content type of the documentation object.
    /// </summary>
    public DocumentationContentType ContentType { get; protected init; }

    protected DocumentationObject()
    {
    }

    protected DocumentationObject(DocumentationObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Id = obj.Id ?? throw new ArgumentException("DocumentationObject.Id cannot be null.", nameof(obj));
        Parent = obj.Parent;
        ContentType = obj.ContentType;
    }
}