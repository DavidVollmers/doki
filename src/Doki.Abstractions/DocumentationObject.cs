namespace Doki;

public record DocumentationObject
{
    public string Id { get; internal init; } = null!;

    public DocumentationObject? Parent { get; internal init; }

    public DocumentationContent Content { get; internal init; }

    public DocumentationObject()
    {
    }

    public DocumentationObject(DocumentationObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Id = obj.Id ?? throw new ArgumentException("DocumentationObject.Id cannot be null.", nameof(obj));
        Parent = obj.Parent;
        Content = obj.Content;
    }
}