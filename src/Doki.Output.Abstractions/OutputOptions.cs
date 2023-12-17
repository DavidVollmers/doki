using System.Text.Json.Serialization;

namespace Doki.Output;

public abstract record OutputOptions
{
    [JsonPropertyName("outputDirectory")] public string? OutputDirectory { get; set; }
    
    public static OutputOptions Default { get; } = new DefaultOutputOptions();
}