using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static string BuildRelativePath(this MarkdownBuilder builder, DokiElement to,
        params string[] additionalParts)
    {
        return builder.BuildRelativePath(to.GetPath(), additionalParts);
    }

    public static Link BuildLinkTo(this MarkdownBuilder builder, DokiElement to, string? textProperty = null)
    {
        var indexFile = to.Content is DokiContent.Assemblies or DokiContent.Assembly or DokiContent.Namespace;

        var relativePath =
            indexFile ? builder.BuildRelativePath(to, "README.md") : builder.BuildRelativePath(to) + ".md";

        if (textProperty == null && to is TypeDocumentationReference typeDocumentationReference)
        {
            textProperty = DokiProperties.FullName;
            if (typeDocumentationReference.Properties?.TryGetValue(DokiProperties.IsDocumented, out var isDocumented) == true &&
                isDocumented is true)
                textProperty = DokiProperties.Name;
        }

        var text = to.Id;
        if (textProperty != null && to.Properties?.TryGetValue(textProperty, out var name) == true && name != null)
            text = name.ToString()!;

        return new Link(text, relativePath);
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