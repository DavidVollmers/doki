using System.Text.Json.Serialization;

namespace Doki.Output;

// ReSharper disable once UnusedTypeParameter
public record OutputOptions<T> where T : IOutput
{
    [JsonPropertyName("outputPath")] public DirectoryInfo OutputDirectory { get; init; } = null!;
}