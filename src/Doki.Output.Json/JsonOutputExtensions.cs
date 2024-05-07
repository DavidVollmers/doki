using Doki.Output.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Doki.Output.Json;

public static class JsonOutputExtensions
{
    [DokiOutputRegistration]
    public static IServiceCollection AddMarkdownOutput(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOutputOptions<JsonOutput>("Doki.Output.Json");

        services.AddSingleton<IOutput, JsonOutput>();

        return services;
    }
}