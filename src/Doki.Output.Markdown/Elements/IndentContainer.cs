using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record IndentContainer(int Indent, bool IndentImmediately = true) : Element
{
    private readonly List<Element> _elements = [];

    public void Add(Element element)
    {
        _elements.Add(element);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        for (var i = 0; i < _elements.Count; i++)
        {
            var element = _elements[i];

            if (i == 0 && !IndentImmediately)
            {
                builder.AppendLine(element.ToString());
            }
            else
            {
                builder.AppendLine($"{new string(' ', Indent * 2)}{element}");
            }
        }

        return builder.ToString();
    }
}