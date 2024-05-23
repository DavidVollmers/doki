using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Doki.Tests.Common;

public sealed class TestOutputLogger(ITestOutputHelper output) : ILogger
{
    public bool HadError { get; private set; }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Error) HadError = true;
        
        output.WriteLine(formatter(state, exception));
        
        if (exception != null) output.WriteLine(exception.ToString());
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