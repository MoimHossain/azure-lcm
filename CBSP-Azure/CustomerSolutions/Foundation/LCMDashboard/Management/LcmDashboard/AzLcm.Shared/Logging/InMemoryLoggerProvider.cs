

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Runtime.Versioning;


namespace AzLcm.Shared.Logging;


[UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private InMemoryLoggerConfiguration _currentConfig;
    private readonly ConcurrentDictionary<string, InMemoryLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly WebLogInMemoryStorage _webLogInMemoryStorage;

    public InMemoryLoggerProvider(
        IOptionsMonitor<InMemoryLoggerConfiguration> config,
        WebLogInMemoryStorage webLogInMemoryStorage)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        _webLogInMemoryStorage = webLogInMemoryStorage;
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new InMemoryLogger(name, GetCurrentConfig, _webLogInMemoryStorage));

    private InMemoryLoggerConfiguration GetCurrentConfig() => _currentConfig;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}

