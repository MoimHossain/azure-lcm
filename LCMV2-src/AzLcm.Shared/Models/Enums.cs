namespace AzLcm.Shared.Models;

public enum ChangeType
{
    Retirement,
    Deprecation,
    BreakingChange,
    SecurityAdvisory,
    NewFeature,
    MigrationRequired,
    Preview,
    GeneralAvailability,
    PriceChange,
    RegionExpansion,
    Unknown
}

public enum Severity
{
    Critical,
    High,
    Medium,
    Low,
    Informational
}

public enum UpdateStatus
{
    New,
    Acknowledged,
    InProgress,
    Remediated,
    Dismissed,
    Overdue
}
