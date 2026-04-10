namespace AzLcm.Shared.Models;

public class DashboardStats
{
    public int TotalUpdates { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public int NewThisWeek { get; set; }
    public int RetirementCount { get; set; }
    public int DeprecationCount { get; set; }
    public int BreakingChangeCount { get; set; }
    public DateTime LastScanTime { get; set; }
}
