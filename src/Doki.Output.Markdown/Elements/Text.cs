using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record Text : Element
{
    public static Text Empty => new(string.Empty);

    protected StringBuilder Builder { get; } = new();

    public bool IsEmpty => Builder.Length == 0;
    
    public Text(string value)
    {
        Builder.Append(value);
    }

    public Text Append(string value)
    {
        Builder.Append(value);
        return this;
    }

    public Text Append(Element element)
    {
        Builder.Append(element);
        return this;
    }

    public override string ToString()
    {
        return Builder.ToString();
    }
}