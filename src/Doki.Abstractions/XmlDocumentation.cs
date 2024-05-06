namespace Doki;

public sealed record XmlDocumentation : DocumentationObject
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    internal DocumentationObject[] InternalContents = [];

    public DocumentationObject[] Contents
    {
        get => InternalContents;
        init => InternalContents = value;
    }
    
    public XmlDocumentation()
    {
        ContentType = DocumentationContentType.Xml;
    }
}