namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static string BuildRelativePath(this MarkdownBuilder builder, DokiElement to, string? extension = null)
    {
        return builder.BuildRelativePath(to.GetPath());
    }

    public static string GetPath(this DokiElement element, string? extension = null)
    {
        var pathParts = new List<string>();

        var current = element;

        while (current.Parent != null)
        {
            pathParts.Add(current.Id);

            current = current.Parent;
        }

        pathParts.Reverse();

        var path = Path.Combine(pathParts.ToArray());
        if (extension != null) path += extension;

        return path;
    }
}