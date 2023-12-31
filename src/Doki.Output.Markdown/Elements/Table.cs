using System.Text;

namespace Doki.Output.Markdown.Elements;

internal record Table : Element
{
    private readonly List<Element[]> _rows = [];
    private readonly Element[] _headers;

    public Table(params Element[] headers)
    {
        _headers = headers;
    }

    public void AddRow(params Element[] elements)
    {
        _rows.Add(elements);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        // ReSharper disable once CoVariantArrayConversion
        sb.AppendLine("|" + string.Join("|", (object[])_headers) + "|");
        sb.AppendLine("|" + string.Join("|", _headers.Select(_ => "---")) + "|");

        foreach (var row in _rows)
        {
            // ReSharper disable once CoVariantArrayConversion
            sb.AppendLine("|" + string.Join("|", (object[])row) + "|");
        }

        return sb.ToString();
    }
}