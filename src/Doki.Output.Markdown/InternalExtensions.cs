using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static Text BuildText(this MarkdownBuilder builder, DocumentationObject obj)
    {
        if (obj is not ContentList { Content: DocumentationContent.XmlDocumentation } contentList)
            throw new ArgumentException("DocumentationObject must be a ContentList with XmlDocumentation content.",
                nameof(obj));

        var text = Text.Empty;

        foreach (var item in contentList.Items)
        {
            if (item is TextContent textContent) text.Append(textContent.Text);
        }

        return text;
    }

    public static string BuildRelativePath(this MarkdownBuilder builder, DocumentationObject to,
        params string[] additionalParts)
    {
        return builder.BuildRelativePath(to.GetPath(), additionalParts);
    }

    public static Element BuildLinkTo(this MarkdownBuilder builder, DocumentationObject to, string? text = null)
    {
        var indexFile = to.Content is DocumentationContent.Assemblies or DocumentationContent.Assembly
            or DocumentationContent.Namespace;

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

        if (to is not TypeDocumentationReference { IsGeneric: true } type ||
            type.GenericArguments.All(a => a.IsGenericParameter))
            return asText ? new Text(text) : new Link(text, relativePath);

        text = text.Split('<')[0];

        var genericArguments = type.GenericArguments.Select(x => builder.BuildLinkTo(x)).ToList();

        var container = Text.Empty.Append(asText ? new Text(text) : new Link(text, relativePath)).Append("<");

        for (var i = 0; i < genericArguments.Count; i++)
        {
            container.Append(genericArguments[i]);

            if (i < genericArguments.Count - 1) container.Append(", ");
        }

        container.Append(">");

        return container;
    }

    public static string GetPath(this DocumentationObject element)
    {
        var pathParts = new List<string>();

        var current = element;
        if (element is TypeDocumentationReference typeDocumentationReference)
        {
            // We cannot use TryGetParent because it will return the wrong namespace/assembly for base type references coming from a different namespace/assembly.
            if (typeDocumentationReference.Assembly != null) pathParts.Add(typeDocumentationReference.Assembly);

            if (typeDocumentationReference.Namespace != null) pathParts.Add(typeDocumentationReference.Namespace);

            pathParts.Add(typeDocumentationReference.Id.Replace('`', '_'));

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