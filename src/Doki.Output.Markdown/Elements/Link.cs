namespace Doki.Output.Markdown.Elements;

internal record Link(string Text, string Url) : Element
{
    public override string ToString()
    {
        return $"[{Text}]({Url})";
    }
}