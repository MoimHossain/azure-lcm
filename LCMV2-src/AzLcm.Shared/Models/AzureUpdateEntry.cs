namespace AzLcm.Shared.Models;

public class AzureUpdateEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public ChangeType ChangeType { get; set; } = ChangeType.Unknown;
    public Severity Severity { get; set; } = Severity.Informational;
    public UpdateStatus Status { get; set; } = UpdateStatus.New;
    public List<string> AffectedServices { get; set; } = [];
    public List<string> Categories { get; set; } = [];
    public string Source { get; set; } = "azure-updates-rss";
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
}
