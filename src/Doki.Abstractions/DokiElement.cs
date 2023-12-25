namespace Doki;

public record DokiElement
{
    public string Id { get; internal init; } = null!;

    public DokiElement? Parent { get; internal init; }

    public DokiContent Content { get; internal init; }

    public DokiElement()
    {
    }

    public DokiElement(DokiElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        Id = element.Id ?? throw new ArgumentException("Doki element ID cannot be null.", nameof(element));
        Parent = element.Parent;
        Content = element.Content;
    }
}