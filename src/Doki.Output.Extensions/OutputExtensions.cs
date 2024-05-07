using Microsoft.Extensions.DependencyInjection;

namespace Doki.Output.Extensions;

public static class OutputExtensions
{
    public static IServiceCollection AddOutputOptions<TOutput>(this IServiceCollection services, string outputType)
        where TOutput : class, IOutput
    {
        return services.AddOutputOptions<TOutput, OutputOptions<TOutput>>(outputType);
    }

    public static IServiceCollection AddOutputOptions<TOutput, TOptions>(this IServiceCollection services,
        string outputType) where TOutput : class, IOutput where TOptions : OutputOptions<TOutput>
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(outputType);

        services.AddSingleton<TOptions>(provider =>
        {
            var optionsProvider = provider.GetService<IOutputOptionsProvider>();
            var options = optionsProvider?.GetOptions<TOutput, TOptions>(outputType);

            return options ?? ActivatorUtilities.CreateInstance<TOptions>(provider);
        });

        return services;
    }
}