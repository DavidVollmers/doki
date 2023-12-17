using System.Text;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal class MarkdownBuilder
{
    private readonly List<Element> _elements = new();

    public MarkdownBuilder Add(Element element)
    {
        _elements.Add(element);

        return this;
    }

    public MarkdownBuilder Add(string text)
    {
        _elements.Add(new Text(text));

        return this;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var element in _elements)
        {
            builder.AppendLine(element.ToString());

            builder.AppendLine();
        }

        return builder.ToString();
    }
}