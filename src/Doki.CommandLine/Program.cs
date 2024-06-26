﻿using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Doki.CommandLine;
using Doki.CommandLine.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddProvider(new AnsiConsoleLoggerProvider());

        builder.SetMinimumLevel(args.Any(a => a == "--debug") ? LogLevel.Trace : LogLevel.Information);
    })
    .ConfigureServices(services => { services.AddDokiCommandLine(); })
    .Build();

var rootCommand = new RootCommand("Doki Command-Line Interface") { Name = "doki" };
rootCommand.AddOption(new Option<bool>("--debug", "Enable debug logging."));

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
        if (e is not TaskCanceledException and not OperationCanceledException)
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        context.ExitCode = 1;
    })
    .Build();

return await parser.InvokeAsync(args);