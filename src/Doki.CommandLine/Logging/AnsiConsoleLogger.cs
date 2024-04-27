using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Doki.CommandLine.Logging;

internal partial class AnsiConsoleLogger : ILogger
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

        message = EscapeRegex().Replace(message, "[[$1]]");

        message = UnescapeRegex().Replace(message, "[$1]$2[/]");

        try
        {
            AnsiConsole.MarkupLine($"{markup}{message}[/]");
        }
        catch (Exception ex)
        {
            if (logLevel == LogLevel.Debug)
            {
                AnsiConsole.MarkupLine("[bold red]Failed to write markup log message:[/]");
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }

            AnsiConsole.WriteLine(message);
        }

        if (exception != null)
        {
            AnsiConsole.WriteException(exception, ExceptionFormats.ShortenEverything);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    [GeneratedRegex(@"\[([^[\]]*)\]")]
    private static partial Regex EscapeRegex();

    [GeneratedRegex(@"\[\[(bold green)\]\](.*)\[\[/\]\]")]
    private static partial Regex UnescapeRegex();
}