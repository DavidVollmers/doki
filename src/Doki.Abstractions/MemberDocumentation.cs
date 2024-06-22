// ReSharper disable InconsistentNaming
namespace Doki;

/// <summary>
/// Represents the documentation for a member.
/// </summary>
public record MemberDocumentation : DocumentationObject
{
    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the namespace of the member.
    /// </summary>
    public string? Namespace { get; init; }

    /// <summary>
    /// Gets the assembly of the member.
    /// </summary>
    public string? Assembly { get; init; }

    internal XmlDocumentation[] InternalSummaries = [];

    /// <summary>
    /// Gets the summary of the member.
    /// </summary>
    public XmlDocumentation[] Summaries
    {
        get => InternalSummaries;
        init => InternalSummaries = value;
    }

    internal XmlDocumentation[] InternalExamples = [];

    /// <summary>
    /// Get the examples of the member.
    /// </summary>
    public XmlDocumentation[] Examples
    {
        get => InternalExamples;
        init => InternalExamples = value;
    }

    internal XmlDocumentation[] InternalRemarks = [];

    /// <summary>
    /// Gets the remarks of the member.
    /// </summary>
    public XmlDocumentation[] Remarks
    {
        get => InternalRemarks;
        init => InternalRemarks = value;
    }

    /// <summary>
    /// Gets a value indicating whether the type is documented.
    /// </summary>
    public bool IsDocumented { get; init; }

    public new DocumentationContentType ContentType { get; init; }
}