using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record SubList(Element Value, int Indent) : List
{
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{new string(' ', Indent * 2)}- {Value}");

        foreach (var item in Items)
        {
            builder.AppendLine($"{new string(' ', (Indent + 1) * 2)}- {item}");
        }

        return builder.ToString();
    }
}