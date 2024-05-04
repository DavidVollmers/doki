using System.Text.Json;
using Doki.Output;
using Doki.Output.Extensions;

namespace Doki.CommandLine;

internal class JsonOutputOptionsProvider : IOutputOptionsProvider
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Dictionary<string, JsonElement?> _options = new();

    public TOptions? GetOptions<TOutput, TOptions>(string outputType) where TOutput : class, IOutput
        where TOptions : class, IOutputOptions<TOutput>
    {
        ArgumentNullException.ThrowIfNull(outputType);

        return _options.TryGetValue(outputType, out var options)
            ? options?.Deserialize<TOptions>(JsonSerializerOptions)
            : null;
    }

    public void AddOptions(string outputType, JsonElement? options)
    {
        _options[outputType] = options;
    }
}