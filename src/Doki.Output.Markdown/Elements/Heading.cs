namespace Doki.Output.Markdown.Elements;

internal record Heading(string Text, int Level) : Element
{
    public override string ToString()
    {
        return $"{new string('#', Level)} {Text}";
    }
}