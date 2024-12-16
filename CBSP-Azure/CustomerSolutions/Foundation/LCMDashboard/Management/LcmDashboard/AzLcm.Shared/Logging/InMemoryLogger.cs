

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AzLcm.Shared.Logging;

public sealed class InMemoryLogger(
    string name,
    Func<InMemoryLoggerConfiguration> getCurrentConfig,
    WebLogInMemoryStorage webLogInMemoryStorage) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        webLogInMemoryStorage.AddLogEntry(new WebLogEntry(name, logLevel, formatter(state, exception)));
    }
}





/*
 * //InMemoryLoggerConfiguration config = getCurrentConfig();
 * if (config.EventId == 0 || config.EventId == eventId.Id)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

            Console.ForegroundColor = originalColor;
            Console.Write($"     {name} - ");

            Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            Console.Write($"{formatter(state, exception)}");

            Console.ForegroundColor = originalColor;
            Console.WriteLine();
        }*/