namespace Doki.Output.Markdown.Elements;

internal abstract record Element
{
    public static Element Separator => new Text("---");
}