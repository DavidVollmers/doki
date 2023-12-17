using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record List : Element
{
    public List<Element> Items { get; init; } = [];

    public override string ToString()
    {
        var builder = new StringBuilder();

        foreach (var item in Items)
        {
            builder.AppendLine($"- {item}");
        }

        return builder.ToString();
    }
}