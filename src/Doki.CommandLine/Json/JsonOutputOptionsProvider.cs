using System.Text.Json;
using Doki.Output;
using Doki.Output.Extensions;

namespace Doki.CommandLine.Json;

internal class JsonOutputOptionsProvider(FileSystemInfo workingDirectory) : IOutputOptionsProvider
{
    private readonly Dictionary<string, JsonElement?> _options = new();

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new DirectoryInfoJsonConverter(workingDirectory)
        }
    };

    public TOptions? GetOptions<TOutput, TOptions>(string outputType) where TOutput : class, IOutput
        where TOptions : class, IOutputOptions<TOutput>
    {
        ArgumentNullException.ThrowIfNull(outputType);

        return _options.TryGetValue(outputType, out var options)
            ? options?.Deserialize<TOptions>(_serializerOptions)
            : null;
    }

    public void AddOptions(string outputType, JsonElement? options)
    {
        _options[outputType] = options;
    }
}