using System.Text.Json.Serialization;

namespace Doki.Output;

public interface IOutputOptions<T> where T : IOutput
{
    [JsonPropertyName("outputPath")] DirectoryInfo OutputDirectory { get; }
}