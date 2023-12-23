namespace Doki.Output.Markdown.Elements;

internal record Code(string Value, string Lang = "csharp") : Element
{
    public override string ToString()
    {
        return $"```{Lang}{Environment.NewLine}{Value}{Environment.NewLine}```";
    }
}