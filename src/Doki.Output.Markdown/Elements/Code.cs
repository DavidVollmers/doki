namespace Doki.Output.Markdown.Elements;

internal record Code(string Value, bool IsInline = false, string Lang = "csharp") : Element
{
    public override string ToString()
    {
        return IsInline ? $"`{Value}`" : $"```{Lang}{Environment.NewLine}{Value}{Environment.NewLine}```";
    }
}