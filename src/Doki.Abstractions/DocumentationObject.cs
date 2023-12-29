namespace Doki;

public abstract record DocumentationObject
{
    public string Id { get; internal init; } = null!;

    public DocumentationObject? Parent { get; internal init; }

    public DocumentationContent Content { get; internal init; }

    protected DocumentationObject()
    {
    }

    protected DocumentationObject(DocumentationObject obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Id = obj.Id ?? throw new ArgumentException("DocumentationObject.Id cannot be null.", nameof(obj));
        Parent = obj.Parent;
        Content = obj.Content;
    }
}