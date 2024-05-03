using System.Text.Json.Serialization;

namespace Doki.Output.ClassLibrary;

public sealed record ClassLibraryOutputOptions : OutputOptions
{
    [JsonPropertyName("namespace")] public string Namespace { get; init; }

    [JsonPropertyName("projectName")] public string? ProjectName { get; init; }

    [JsonPropertyName("targetFramework")] public string TargetFramework { get; init; }

    [JsonPropertyName("targetFrameworks")] public string[]? TargetFrameworks { get; init; }

    public ClassLibraryOutputOptions()
    {
        Namespace = "Doki.Content";
        TargetFramework = "net8.0";
    }
}