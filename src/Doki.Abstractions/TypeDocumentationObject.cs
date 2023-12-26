namespace Doki;

public abstract record TypeDocumentationObject : DocumentationObject
{
    public bool IsGeneric { get; internal init; }

    public string? Namespace { get; internal init; }

    public string? Assembly { get; internal init; }

    public IEnumerable<GenericTypeArgumentDocumentation> GenericArguments { get; internal set; } =
        Array.Empty<GenericTypeArgumentDocumentation>();

    public TypeDocumentationObject()
    {
    }

    public TypeDocumentationObject(TypeDocumentationObject obj) : base(obj)
    {
        IsGeneric = obj.IsGeneric;
        Namespace = obj.Namespace;
        Assembly = obj.Assembly;
    }
}