using Doki.Output.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Doki.Output.ClassLibrary;

public static class ClassLibraryOutputExtensions
{
    [DokiOutputRegistration]
    public static IServiceCollection AddClassLibraryOutput(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        //TODO add options

        services.AddSingleton<IOutput, ClassLibraryOutput>();

        return services;
    }
}