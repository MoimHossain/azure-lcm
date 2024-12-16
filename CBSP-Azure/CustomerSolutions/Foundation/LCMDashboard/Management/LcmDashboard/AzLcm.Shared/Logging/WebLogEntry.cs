

using Microsoft.Extensions.Logging;

namespace AzLcm.Shared.Logging;

public record WebLogEntry(string Name, DateTimeOffset Timestamp, LogLevel LogLevel, string Message);
