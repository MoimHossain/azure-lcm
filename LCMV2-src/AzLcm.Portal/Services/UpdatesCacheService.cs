using AzLcm.Shared.Models;
using AzLcm.Shared.Services;

namespace AzLcm.Portal.Services;

public class UpdatesCacheService
{
    private readonly AzureUpdatesFeedService _feedService;
    private List<AzureUpdateEntry> _cachedUpdates = [];
    private DateTime _lastFetch = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public UpdatesCacheService(AzureUpdatesFeedService feedService)
    {
        _feedService = feedService;
    }

    public async Task<List<AzureUpdateEntry>> GetUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedUpdates.Count > 0 && DateTime.UtcNow - _lastFetch < _cacheExpiry)
            return _cachedUpdates;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_cachedUpdates.Count > 0 && DateTime.UtcNow - _lastFetch < _cacheExpiry)
                return _cachedUpdates;

            _cachedUpdates = await _feedService.FetchUpdatesAsync(cancellationToken);
            _lastFetch = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }

        return _cachedUpdates;
    }

    public async Task<DashboardStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var updates = await GetUpdatesAsync(cancellationToken);
        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

        return new DashboardStats
        {
            TotalUpdates = updates.Count,
            CriticalCount = updates.Count(u => u.Severity == Severity.Critical),
            HighCount = updates.Count(u => u.Severity == Severity.High),
            MediumCount = updates.Count(u => u.Severity == Severity.Medium),
            LowCount = updates.Count(u => u.Severity == Severity.Low),
            NewThisWeek = updates.Count(u => u.PublishedDate >= oneWeekAgo),
            RetirementCount = updates.Count(u => u.ChangeType == ChangeType.Retirement),
            DeprecationCount = updates.Count(u => u.ChangeType == ChangeType.Deprecation),
            BreakingChangeCount = updates.Count(u => u.ChangeType == ChangeType.BreakingChange),
            LastScanTime = _lastFetch
        };
    }

    public async Task<AzureUpdateEntry?> GetUpdateByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var updates = await GetUpdatesAsync(cancellationToken);
        return updates.FirstOrDefault(u => u.Id == id);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _cachedUpdates = await _feedService.FetchUpdatesAsync(cancellationToken);
            _lastFetch = DateTime.UtcNow;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}