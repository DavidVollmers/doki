namespace Doki.Output.Markdown.Elements;

internal record Text(string Value) : Element
{
    public override string ToString()
    {
        return Value;
    }
}