
using Microsoft.Extensions.Logging;

namespace AzLcm.Shared.Logging;

public sealed class InMemoryLoggerConfiguration
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.Green
    };
}