using System.Text.Json.Serialization;

namespace Doki.Output.ClassLibrary;

public sealed record ClassLibraryOutputOptions : IOutputOptions<ClassLibraryOutput>
{
    [JsonPropertyName("namespace")] public string Namespace { get; init; } = "Doki.Content";

    [JsonPropertyName("projectName")] public string? ProjectName { get; init; }

    [JsonPropertyName("targetFramework")] public string TargetFramework { get; init; } = "net8.0";

    [JsonPropertyName("targetFrameworks")] public string[]? TargetFrameworks { get; init; }
    
    public DirectoryInfo OutputDirectory { get; init; } = null!;
}