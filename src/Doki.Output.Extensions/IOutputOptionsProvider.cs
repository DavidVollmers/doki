namespace Doki.Output.Extensions;

public interface IOutputOptionsProvider
{
    TOptions RequireOptions<TOutput, TOptions>(string outputType) where TOutput : class, IOutput
        where TOptions : class, IOutputOptions<TOutput>;
}