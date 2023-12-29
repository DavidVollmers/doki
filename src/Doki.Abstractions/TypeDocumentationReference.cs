namespace Doki;

public record TypeDocumentationReference : MemberDocumentation
{
    public bool IsGeneric { get; internal init; }
    
    public string FullName { get; internal init; } = null!;
    
    public bool IsDocumented { get; internal init; }

    public bool IsMicrosoft { get; internal init; }

    public TypeDocumentationReference? BaseType { get; internal set; }

    public GenericTypeArgumentDocumentation[] GenericArguments { get; internal set; } =
        Array.Empty<GenericTypeArgumentDocumentation>();
    
    public TypeDocumentationReference()
    {
        Content = DocumentationContent.TypeReference;
    }

    public TypeDocumentationReference(TypeDocumentationReference reference) : base(reference)
    {
        Content = DocumentationContent.TypeReference;
        IsGeneric = reference.IsGeneric;
        GenericArguments = reference.GenericArguments;
        FullName = reference.FullName;
        IsDocumented = reference.IsDocumented;
        IsMicrosoft = reference.IsMicrosoft;
        BaseType = reference.BaseType;
    }
}