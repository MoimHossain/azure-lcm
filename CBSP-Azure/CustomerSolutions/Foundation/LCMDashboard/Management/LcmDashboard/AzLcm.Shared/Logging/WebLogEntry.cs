

using Microsoft.Extensions.Logging;

namespace AzLcm.Shared.Logging;

public record WebLogEntry(string Name, LogLevel LogLevel, string Message);
