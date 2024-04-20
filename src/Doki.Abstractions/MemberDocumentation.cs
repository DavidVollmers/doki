namespace Doki;

/// <summary>
/// Represents the documentation for a member.
/// </summary>
public record MemberDocumentation : DocumentationObject
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; internal init; } = null!;

    /// <summary>
    /// Gets the namespace of the member.
    /// </summary>
    public string? Namespace { get; internal init; }

    /// <summary>
    /// Gets the assembly of the member.
    /// </summary>
    public string? Assembly { get; internal init; }
    
    /// <summary>
    /// Gets the summary of the member.
    /// </summary>
    public DocumentationObject? Summary { get; internal set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberDocumentation"/> class.
    /// </summary>
    public MemberDocumentation()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberDocumentation"/> class.
    /// </summary>
    /// <param name="obj">The object to copy.</param>
    /// <exception cref="ArgumentException"></exception>
    public MemberDocumentation(MemberDocumentation obj) : base(obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Name = obj.Name ?? throw new ArgumentException("MemberDocumentation.Name cannot be null.", nameof(obj));
        Namespace = obj.Namespace;
        Assembly = obj.Assembly;
        Summary = obj.Summary;
    }
}