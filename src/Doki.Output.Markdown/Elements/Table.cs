using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record Table : Element
{
    private readonly List<Element[]> _rows = [];
    
    public void AddRow(params Element[] elements)
    {
        _rows.Add(elements);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("|" + string.Join("|", _rows[0].Select(_ => "   ")) + "|");
        sb.AppendLine("|" + string.Join("|", _rows[0].Select(_ => "---")) + "|");

        foreach (var row in _rows)
        {
            // ReSharper disable once CoVariantArrayConversion
            sb.AppendLine("|" + string.Join("|", (object[]) row) + "|");
        }

        return sb.ToString();
    }
}