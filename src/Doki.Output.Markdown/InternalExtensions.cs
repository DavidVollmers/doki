using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal static class InternalExtensions
{
    public static Text BuildBreadcrumbs(this MarkdownBuilder builder, DocumentationObject obj)
    {
        var text = Text.Empty;

        var parents = new List<Element>();

        var current = obj;
        while (current.Parent != null)
        {
            parents.Add(builder.BuildLinkTo(current.Parent));

            current = current.Parent;
        }

        parents.Reverse();
        foreach (var parent in parents)
        {
            text.Append(parent);

            text.Append(" / ");
        }

        return text;
    }

    public static Text BuildText(this MarkdownBuilder builder, DocumentationObject obj)
    {
        if (obj is not XmlDocumentation { ContentType: DocumentationContentType.Xml } xmlDocumentation)
            throw new ArgumentException("DocumentationObject must be a ContentList with XmlDocumentation content.",
                nameof(obj));

        var text = Text.Empty;

        foreach (var content in xmlDocumentation.Contents)
        {
            switch (content)
            {
                case TextContent textContent:
                    if (textContent.Text != "." && textContent.Text != "?" && textContent.Text != "!" &&
                        textContent.Text != "," && textContent.Text != ":" && textContent.Text != ";")
                        text.Space();
                    text.Append(textContent.Text);
                    break;
                case Link link:
                    text.Space();
                    text.Append(new Elements.Link(link.Text, link.Url));
                    break;
                case CodeBlock codeBlock:
                    text.NewLine();
                    text.Append(new Code(codeBlock.Code, Lang: codeBlock.Language));
                    text.NewLine();
                    break;
                case TypeDocumentationReference typeDocumentationReference:
                    text.Space();
                    text.Append(builder.BuildLinkTo(typeDocumentationReference));
                    break;
                case MemberDocumentation memberDocumentation:
                    text.Space();
                    text.Append(builder.BuildLinkTo(memberDocumentation));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Unsupported {nameof(DocumentationObject)} type: {content.GetType().Name}");
            }
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
        var asText = false;
        string relativePath;
        switch (to)
        {
            case TypeDocumentationReference typeDocumentationReference:
            {
                text ??= typeDocumentationReference.IsDocumented
                    ? typeDocumentationReference.Name
                    : typeDocumentationReference.FullName;

                asText = typeDocumentationReference is { IsDocumented: false, IsMicrosoft: false };

                if (typeDocumentationReference.IsMicrosoft)
                {
                    relativePath =
                        $"https://learn.microsoft.com/en-us/dotnet/api/{typeDocumentationReference.FullName}";
                }
                else
                {
                    relativePath = builder.BuildRelativePath(to, "README.md");
                }

                break;
            }
            case MemberDocumentation memberDocumentation:
            {
                text ??= memberDocumentation.Name;

                asText = !memberDocumentation.IsDocumented;

                relativePath = builder.BuildRelativePath(memberDocumentation) + ".md";
                break;
            }
            default:
            {
                var indexFile = to.ContentType is DocumentationContentType.Root or DocumentationContentType.Assembly
                    or DocumentationContentType.Namespace;

                relativePath = indexFile
                    ? builder.BuildRelativePath(to, "README.md")
                    : builder.BuildRelativePath(to) + ".md";
                break;
            }
        }

        text ??= to.Id;
        if (text == "root") text = "Packages";

        if (to is not TypeDocumentationReference { IsGeneric: true } type ||
            type.GenericArguments.All(a => a.IsGenericParameter))
            return asText ? new Text(text) : new Elements.Link(text, relativePath);

        text = text.Split('<')[0];

        var genericArguments = type.GenericArguments.Select(x => builder.BuildLinkTo(x)).ToList();

        var container = Text.Empty.Append(asText ? new Text(text) : new Elements.Link(text, relativePath)).Append("<");

        for (var i = 0; i < genericArguments.Count; i++)
        {
            container.Append(genericArguments[i]);

            if (i < genericArguments.Count - 1) container.Append(", ");
        }

        container.Append(">");

        return container;
    }

    private static string GetPathId(this DocumentationObject documentationObject)
    {
        return documentationObject.Id.Replace('`', '_');
    }

    private static List<string> GetPathList(DocumentationObject documentationObject)
    {
        var pathList = new List<string>();

        var current = documentationObject;
        switch (documentationObject)
        {
            case TypeDocumentationReference typeDocumentationReference:
            {
                // We cannot use TryGetParent because it will return the wrong namespace/assembly for base type references coming from a different namespace/assembly.
                if (typeDocumentationReference.Assembly != null) pathList.Add(typeDocumentationReference.Assembly);

                if (typeDocumentationReference.Namespace != null) pathList.Add(typeDocumentationReference.Namespace);

                pathList.Add(typeDocumentationReference.GetPathId());

                return pathList;
            }
            case MemberDocumentation { Parent: not null } memberDocumentation:
            {
                var parentPathList = GetPathList(memberDocumentation.Parent);

                parentPathList.Add(memberDocumentation.GetPathId());

                return parentPathList;
            }
        }

        while (current.Parent != null)
        {
            pathList.Add(current.Id);

            current = current.Parent;
        }

        pathList.Reverse();

        return pathList;
    }

    public static string GetPath(this DocumentationObject documentationObject)
    {
        return GetPathList(documentationObject).CombineToPath();
    }

    public static string CombineToPath(this ICollection<string> parts)
    {
        return parts.Count == 0 ? string.Empty : string.Join('/', parts);
    }
}