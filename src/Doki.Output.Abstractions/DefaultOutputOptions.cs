using System.Text.Json.Serialization;

namespace Doki.Output;

/// <summary>
/// Default output options.
/// </summary>
public sealed record DefaultOutputOptions<T> : IOutputOptions<T> where T : IOutput
{
    [JsonPropertyName("outputPath")] public DirectoryInfo OutputDirectory { get; init; } = null!;
}