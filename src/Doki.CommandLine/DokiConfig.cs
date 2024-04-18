using System.Text.Json;
using System.Text.Json.Serialization;

namespace Doki.CommandLine;

internal record DokiConfig
{
    [JsonInclude]
    [JsonPropertyName("$schema")]
    internal string? Schema { get; init; }

    [JsonPropertyName("input")] public string[]? Inputs { get; init; }

    [JsonPropertyName("output")] public DokiConfigOutput[]? Outputs { get; init; }

    [JsonPropertyName("filter")] public IDictionary<string, string>? Filter { get; init; }

    [JsonPropertyName("includeInheritedMembers")]
    public bool IncludeInheritedMembers { get; init; }

    public record DokiConfigOutput
    {
        public string Type { get; init; } = null!;

        public string? From { get; init; } = null!;

        public JsonElement? Options { get; init; }
    }
}