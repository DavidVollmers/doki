using System.CommandLine;
using Doki.CommandLine.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Doki.CommandLine;

internal static class CommandLineExtensions
{
    public static IServiceCollection AddDokiCommandLine(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<Command, BuildCommand>();
        serviceCollection.AddSingleton<Command, InitCommand>();

        return serviceCollection;
    }
}