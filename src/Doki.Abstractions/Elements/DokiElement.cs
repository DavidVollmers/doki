namespace Doki.Elements;

public record DokiElement
{
    public Guid Id { get; internal init; }

    public string Name { get; internal init; }

    public DokiElement? Parent { get; internal init; }
}