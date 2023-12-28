namespace Doki;

public record ExampleDocumentation : DocumentationObject
{
    public DocumentationObject? Documentation { get; internal set; }
}