using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record SubList(string Value, int Indent) : List
{
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{new string(' ', Indent)}- {Value}");

        foreach (var item in Items)
        {
            builder.AppendLine($"{new string(' ', Indent + 1)}- {item}");
        }

        return builder.ToString();
    }
}