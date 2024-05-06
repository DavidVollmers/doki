namespace Doki;

public sealed record NamespaceDocumentation : DocumentationObject
{
    public string Name { get; init; } = null!;

    public string? Description { get; init; }

    internal TypeDocumentation[] InternalTypes = [];

    public TypeDocumentation[] Types
    {
        get => InternalTypes;
        init => InternalTypes = value;
    }

    public NamespaceDocumentation()
    {
        Content = DocumentationContentType.Namespace;
    }
}