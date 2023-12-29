namespace Doki;

public sealed record ConstructorDocumentation : MemberDocumentation
{
    public DocumentationObject? Description { get; internal set; }
}