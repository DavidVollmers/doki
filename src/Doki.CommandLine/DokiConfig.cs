using System.Text.Json.Serialization;

namespace Doki.CommandLine;

internal class DokiConfig
{
    [JsonPropertyName("output")] public DokiConfigOutput[]? Outputs { get; set; }

    public class DokiConfigOutput
    {
        public string? Type { get; set; }

        public string? From { get; set; }
    }
}