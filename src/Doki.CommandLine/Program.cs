using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Doki.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddDokiCommandLine(); })
    .Build();

var rootCommand = new RootCommand("Doki Command-Line Interface") { Name = "doki" };

var commands = host.Services.GetServices<Command>();
foreach (var command in commands)
{
    rootCommand.Add(command);
}

if (args.Length == 0)
{
    AnsiConsole.Write(new FigletText("Doki").Color(new Color(59, 53, 97)));
}

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseExceptionHandler((e, context) =>
    {
        AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        context.ExitCode = 1;
    })
    .Build();

return await parser.InvokeAsync(args);