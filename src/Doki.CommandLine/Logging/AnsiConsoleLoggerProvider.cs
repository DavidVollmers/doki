using Microsoft.Extensions.Logging;

namespace Doki.CommandLine.Logging;

internal class AnsiConsoleLoggerProvider : ILoggerProvider
{
    private static readonly AnsiConsoleLogger Logger = new();
    
    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName)
    {
        return Logger;
    }
}