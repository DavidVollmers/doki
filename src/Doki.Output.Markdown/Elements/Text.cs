using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record Text : Element
{
    public static Text Empty => new(string.Empty);

    private readonly StringBuilder _builder = new();

    public bool IsEmpty => _builder.Length == 0;

    public bool IsBold { get; init; }

    public Text(string value)
    {
        _builder.Append(value);
    }

    public Text Append(string value)
    {
        _builder.Append(value);
        return this;
    }

    public Text Append(Element element)
    {
        _builder.Append(element);
        return this;
    }

    public override string ToString()
    {
        return GetText();
    }

    protected string GetText()
    {
        if (IsEmpty) return string.Empty;
        var escaped = _builder.ToString().Replace("<", "&lt;").Replace(">", "&gt;");
        return IsBold ? $"**{escaped}**" : escaped;
    }
}