using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doki.CommandLine;

internal record DokiConfig
{
    [JsonPropertyName("output")] public DokiConfigOutput[]? Outputs { get; set; }

    public record DokiConfigOutput
    {
        public string? Type { get; set; }

        public string? From { get; set; }

        public JsonElement? Options { get; set; }
    }
}