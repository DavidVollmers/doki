namespace Doki;

public record DokiElement
{
    public string Id { get; internal init; } = null!;

    public DokiElement? Parent { get; internal init; }

    public DokiContent Content { get; internal init; }

    public IReadOnlyDictionary<string, object?>? Properties { get; internal init; }
}