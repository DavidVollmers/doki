using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static string BuildRelativePath(this MarkdownBuilder builder, DokiElement to,
        params string[] additionalParts)
    {
        return builder.BuildRelativePath(to.GetPath(), additionalParts);
    }

    public static Element BuildLinkTo(this MarkdownBuilder builder, DokiElement to, string? text = null)
    {
        var indexFile = to.Content is DokiContent.Assemblies or DokiContent.Assembly or DokiContent.Namespace;

        var asText = false;
        string? relativePath = null;
        if (to is TypeDocumentationReference typeDocumentationReference)
        {
            text ??= typeDocumentationReference.IsDocumented
                ? typeDocumentationReference.Name
                : typeDocumentationReference.FullName;

            asText = typeDocumentationReference is { IsDocumented: false, IsMicrosoft: false };

            if (typeDocumentationReference.IsMicrosoft)
            {
                relativePath = $"https://learn.microsoft.com/en-us/dotnet/api/{typeDocumentationReference.FullName}";
            }
        }

        relativePath ??= indexFile ? builder.BuildRelativePath(to, "README.md") : builder.BuildRelativePath(to) + ".md";

        text ??= to.Id;

        if (asText) return new Text(text);
        return new Link(text, relativePath);
    }

    public static string GetPath(this DokiElement element)
    {
        var pathParts = new List<string>();

        var current = element;
        if (element is TypeDocumentationReference typeDocumentationReference)
        {
            // We cannot use TryGetParent because it will return the wrong namespace/assembly for base type references coming from a different namespace/assembly.
            if (typeDocumentationReference.Assembly != null) pathParts.Add(typeDocumentationReference.Assembly);

            if (typeDocumentationReference.Namespace != null) pathParts.Add(typeDocumentationReference.Namespace);

            pathParts.Add(typeDocumentationReference.Id);

            return pathParts.CombineToPath();
        }

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