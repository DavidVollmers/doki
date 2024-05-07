using Doki.Output.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Doki.Output.Markdown;

public static class MarkdownOutputExtensions
{
    [DokiOutputRegistration]
    public static IServiceCollection AddMarkdownOutput(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOutputOptions<MarkdownOutput>("Doki.Output.Markdown");

        services.AddSingleton<IOutput, MarkdownOutput>();

        return services;
    }
}