namespace Doki;

public record ExampleDocumentation : DocumentationObject
{
    public string? Text { get; internal init; }
    
    public string? Code { get; internal init; }
}