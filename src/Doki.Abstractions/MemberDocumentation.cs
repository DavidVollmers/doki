namespace Doki;

public record MemberDocumentation : DocumentationObject
{
    public string Name { get; internal init; } = null!;

    public string? Namespace { get; internal init; }

    public string? Assembly { get; internal init; }
    
    public DocumentationObject? Summary { get; internal set; }
    
    public MemberDocumentation()
    {
    }
    
    public MemberDocumentation(MemberDocumentation obj) : base(obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Name = obj.Name ?? throw new ArgumentException("MemberDocumentation.Name cannot be null.", nameof(obj));
        Namespace = obj.Namespace;
        Assembly = obj.Assembly;
        Summary = obj.Summary;
    }
}