using System.Text;
using System.Text.RegularExpressions;

namespace Doki;

internal static class StringExtensions
{
    private static string TrimIndentation(this string str)
    {
        var lines = str.TrimEmptyLines();

        if (lines.Length == 1) return lines[0].Trim();

        var indentRegex = new Regex(@"^\s+");

        var commonIndent = lines
            .Select(l =>
            {
                if (string.IsNullOrWhiteSpace(l)) return int.MaxValue;
                var match = indentRegex.Match(l);
                return match.Success ? match.Length : 0;
            })
            .Min();

        if (commonIndent == int.MaxValue) return string.Empty;

        var builder = new StringBuilder();
        foreach (var line in lines)
        {
            if (commonIndent > line.Length) builder.AppendLine();
            else builder.AppendLine(line[commonIndent..].TrimEnd());
        }

        return builder.ToString();
    }

    private static string[] TrimEmptyLines(this string str)
    {
        var lines = str.Split("\r\n");
        var emptyLineRegex = new Regex(@"^\s*$");
        var firstNonEmptyLine = Array.FindIndex(lines, l => !emptyLineRegex.IsMatch(l));
        var lastNonEmptyLine = Array.FindLastIndex(lines, l => !emptyLineRegex.IsMatch(l));
        return lines[firstNonEmptyLine..(lastNonEmptyLine + 1)].Select(s => s.Replace("\n", string.Empty)).ToArray();
    }
}