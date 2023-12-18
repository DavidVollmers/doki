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
            if (item is List)
            {
                builder.AppendLine(item.ToString());
                continue;
            }
            
            builder.AppendLine($"- {item}");
        }

        return builder.ToString();
    }
}