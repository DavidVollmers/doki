using System.Text;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal class MarkdownBuilder
{
    private readonly string[] _currentPathParts;
    private readonly List<Element> _elements = [];

    public MarkdownBuilder(string currentPath)
    {
        var currentPathParts = currentPath.Split('/');
        _currentPathParts = currentPathParts.Where(p => !p.EndsWith(".md")).ToArray();
    }

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
        var path = to.Split('/').Where(p => !p.EndsWith(".md")).ToArray();

        var currentPathIndex = _currentPathParts.Length - 1;
        var pathIndex = 0;

        //TODO fix this
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

        return string.Join('/', resultPath);
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