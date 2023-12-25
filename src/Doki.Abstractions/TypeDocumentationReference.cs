namespace Doki;

public record TypeDocumentationReference : DokiElement
{
    public string Name { get; internal init; } = null!;

    public string FullName { get; internal init; } = null!;

    public string Definition { get; internal init; } = null!;

    public bool IsDocumented { get; internal init; }

    public bool IsMicrosoft { get; internal init; }

    public string? Namespace { get; internal init; }

    public string? Assembly { get; internal init; }

    public TypeDocumentationReference? BaseType { get; internal set; }
    
    public TypeDocumentationReference()
    {
        Content = DokiContent.TypeReference;
    }

    public TypeDocumentationReference(TypeDocumentationReference reference) : base(reference)
    {
        Content = DokiContent.TypeReference;
        Name = reference.Name;
        FullName = reference.FullName;
        Definition = reference.Definition;
        IsDocumented = reference.IsDocumented;
        IsMicrosoft = reference.IsMicrosoft;
        BaseType = reference.BaseType;
        Namespace = reference.Namespace;
        Assembly = reference.Assembly;
    }
}