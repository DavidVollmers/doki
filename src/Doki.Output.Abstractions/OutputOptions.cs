using System.Text.Json.Serialization;

namespace Doki.Output;

/// <summary>
/// Options for the output.
/// </summary>
public abstract record OutputOptions
{
    /// <summary>
    /// Gets the output path.
    /// </summary>
    [JsonPropertyName("outputPath")] public string? OutputPath { get; init; }
    
    /// <summary>
    /// Gets the default output options.
    /// </summary>
    public static OutputOptions Default { get; } = new DefaultOutputOptions();
}