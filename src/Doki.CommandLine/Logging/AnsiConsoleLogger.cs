﻿using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Doki.CommandLine.Logging;

internal class AnsiConsoleLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var markup = logLevel switch
        {
            LogLevel.Trace => "[grey]",
            LogLevel.Debug => "[grey]",
            LogLevel.Information => "[white]",
            LogLevel.Warning => "[yellow]",
            LogLevel.Error => "[red]",
            LogLevel.Critical => "[red]",
            LogLevel.None => "[white]",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };
        
        var message = formatter(state, exception);
        
        AnsiConsole.MarkupLine($"{markup}{message}[/]");
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
}