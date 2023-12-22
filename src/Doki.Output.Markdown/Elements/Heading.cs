namespace Doki.Output.Markdown.Elements;

internal record Heading : Text
{
    private readonly int _level;

    public Heading(string text, int level) : base(text)
    {
        _level = level;
    }

    public override string ToString()
    {
        return $"{new string('#', _level)} {Builder}";
    }
}