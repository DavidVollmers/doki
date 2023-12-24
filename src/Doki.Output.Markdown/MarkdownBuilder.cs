using System.Text;
using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal class MarkdownBuilder
{
    private static readonly Func<string, bool>
        PathPartFilter = p => !string.IsNullOrWhiteSpace(p) && !p.EndsWith(".md");

    private readonly string[] _currentPathParts;
    private readonly List<Element> _elements = [];

    public MarkdownBuilder(string currentPath)
    {
        var currentPathParts = currentPath.Split('/');
        _currentPathParts = currentPathParts.Where(PathPartFilter).ToArray();
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

    public string BuildRelativePath(string to, params string[] additionalParts)
    {
        var path = to.Split('/').Where(PathPartFilter).ToArray();

        var commonPathParts = _currentPathParts.Zip(path, (a, b) => a == b).TakeWhile(x => x).Count();

        var relativePathParts = new List<string>();

        for (var i = 0; i < _currentPathParts.Length - commonPathParts; i++)
        {
            relativePathParts.Add("..");
        }

        relativePathParts.AddRange(path.Skip(commonPathParts));
        
        relativePathParts.AddRange(additionalParts);

        return relativePathParts.CombineToPath();
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