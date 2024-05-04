using System.Text.Json;
using Doki.Output;
using Doki.Output.Extensions;

namespace Doki.CommandLine;

internal class JsonOutputOptionsProvider : IOutputOptionsProvider
{
    public TOptions RequireOptions<TOutput, TOptions>(string outputType) where TOutput : class, IOutput
        where TOptions : class, IOutputOptions<TOutput>
    {
        throw new NotImplementedException();
    }

    public void AddOptions(string outputType, JsonElement? options)
    {
        throw new NotImplementedException();
    }
}