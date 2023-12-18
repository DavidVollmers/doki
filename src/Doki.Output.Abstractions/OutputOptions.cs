using System.Text.Json.Serialization;

namespace Doki.Output;

public abstract record OutputOptions
{
    [JsonPropertyName("outputPath")] public string? OutputPath { get; set; }
    
    public static OutputOptions Default { get; } = new DefaultOutputOptions();
}