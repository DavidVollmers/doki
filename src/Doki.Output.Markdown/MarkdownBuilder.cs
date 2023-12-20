using System.Text;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal class MarkdownBuilder(string currentPath)
{
    private readonly string[] _currentPathParts = currentPath.Split(Path.DirectorySeparatorChar);
    private readonly List<Element> _elements = [];

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

    public string BuildRelativePath(string to)
    {
        var path = to.Split(Path.DirectorySeparatorChar);

        var currentPathIndex = _currentPathParts.Length - 1;
        var pathIndex = 0;

        while (currentPathIndex >= 0 && pathIndex < path.Length &&
               _currentPathParts[currentPathIndex] == path[pathIndex])
        {
            currentPathIndex--;
            pathIndex++;
        }

        var resultPath = new List<string>();

        for (var i = 0; i < currentPathIndex; i++)
        {
            resultPath.Add("..");
        }

        for (var i = pathIndex; i < path.Length; i++)
        {
            resultPath.Add(path[i]);
        }

        return Path.Combine(resultPath.ToArray());
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