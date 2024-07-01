using Doki.Output.Markdown.Elements;

namespace Doki.Output.Markdown;

internal static class MarkdownBuilderExtensions
{
    public static void AddHeadingWithList<T>(this MarkdownBuilder markdown, string heading, T[] items, int level = 2)
        where T : DocumentationObject
    {
        if (items.Length == 0) return;

        markdown.Add(new Heading(heading, level));

        markdown.Add(new List
        {
            Items = items.Select(x => markdown.BuildLinkTo(x)).ToList()
        });
    }

    public static void AddHeadingWithXmlDocumentation(this MarkdownBuilder markdown, string heading,
        XmlDocumentation[] xmlDocumentations, int level = 2)
    {
        if (xmlDocumentations.Length == 0) return;

        markdown.Add(new Heading(heading, level));

        foreach (var xmlDocumentation in xmlDocumentations)
        {
            markdown.Add(markdown.BuildText(xmlDocumentation));
        }
    }

    public static void AddBoldXmlDocumentation(this MarkdownBuilder markdown, XmlDocumentation[] xmlDocumentations)
    {
        if (xmlDocumentations.Length == 0) return;

        foreach (var xmlDocumentation in xmlDocumentations)
        {
            var text = markdown.BuildText(xmlDocumentation);
            text.IsBold = true;
            markdown.Add(text);
        }
    }

    public static void AddTextWithTypeDocumentationReferences(this MarkdownBuilder markdown, string text,
        TypeDocumentationReference[] typeDocumentationReferences, string separator = ", ")
    {
        if (typeDocumentationReferences.Length == 0) return;

        var textElement = new Text(text);
        for (var i = 0; i < typeDocumentationReferences.Length; i++)
        {
            if (i != 0) textElement.Append(separator);
            textElement.Append(markdown.BuildLinkTo(typeDocumentationReferences[i]));
        }

        markdown.Add(textElement);
    }

    public static void AddHeadingWithMemberTable<T>(this MarkdownBuilder markdown, string heading, T[] items,
        int level = 2) where T : MemberDocumentation
    {
        if (items.Length == 0) return;

        markdown.Add(new Heading(heading, level));

        var table = new Table(new Text("   "), new Text("Summary"));
        foreach (var item in items)
        {
            var text = Text.Empty;
            foreach (var summary in item.Summaries)
            {
                text.Append(markdown.BuildText(summary));
            }
            
            // var link = markdown.BuildLinkTo(item);
    
            table.AddRow(new Text(item.Name), text);
        }

        markdown.Add(table);
    }
}