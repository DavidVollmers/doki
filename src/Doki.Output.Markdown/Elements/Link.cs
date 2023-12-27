namespace Doki.Output.Markdown.Elements;

internal record Link : Text
{
    public string Url { get; }

    public Link(string text, string url) : base(text)
    {
        Url = url;
    }

    public override string ToString()
    {
        return $"[{GetText()}]({Url})";
    }
}