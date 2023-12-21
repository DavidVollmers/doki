using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record Text : Element
{
    public static readonly Text Empty = new(string.Empty);

    private readonly StringBuilder _builder = new();

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
        return _builder.ToString();
    }
}