using Doki.Output.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Doki.Output.Markdown;

public static class MarkdownOutputExtensions
{
    [DokiOutputRegistration]
    public static IServiceCollection AddMarkdownOutput(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        //TODO add options

        services.AddSingleton<IOutput, MarkdownOutput>();

        return services;
    }
}