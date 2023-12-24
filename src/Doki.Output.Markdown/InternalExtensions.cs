namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static string BuildRelativePath(this MarkdownBuilder builder, DokiElement to,
        params string[] additionalParts)
    {
        return builder.BuildRelativePath(to.GetPath(), additionalParts);
    }

    public static string GetPath(this DokiElement element)
    {
        var pathParts = new List<string>();

        var current = element;

        while (current.Parent != null)
        {
            pathParts.Add(current.Id);

            current = current.Parent;
        }

        pathParts.Reverse();

        return pathParts.CombineToPath();
    }

    public static string CombineToPath(this ICollection<string> parts)
    {
        return parts.Count == 0 ? string.Empty : string.Join('/', parts);
    }
}