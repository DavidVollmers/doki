namespace Doki.Output.Markdown.Elements;

internal record Text(string Value) : Element
{
    public static readonly Text Empty = new(string.Empty);

    public override string ToString()
    {
        return Value;
    }
}