namespace Doki.Output.Extensions;

public interface IOutputOptionsProvider
{
    TOptions? GetOptions<TOutput, TOptions>(string outputType) where TOutput : class, IOutput
        where TOptions : class, IOutputOptions<TOutput>;
}