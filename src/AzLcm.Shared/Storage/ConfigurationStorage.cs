
using AzLcm.Shared.ServiceHealth;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AzLcm.Shared.Storage;

public class ConfigurationStorage(
    DaemonConfig daemonConfig, 
    ILogger<ConfigurationStorage> logger, 
    AzureCredentialProvider azureCredentialProvider) : StorageBase
{
    protected override ILogger Logger => logger;
    protected override AzureCredentialProvider GetAzureCredentialProvider() => azureCredentialProvider;

    protected override string GetStorageAccountName() => daemonConfig.StorageAccountName;

    protected override string GetStorageTableName() => daemonConfig.ConfigTableName;

    public async Task<GeneralConfig> LoadGeneralConfigAsync(CancellationToken stoppingToken)
    {
        var defaultValue = new GeneralConfig
        {
            DelayMilliseconds = 1000 * 60 * 5,
            ProcessServiceHealth = daemonConfig.ProcessServiceHealth,
            ProcessPolicy = daemonConfig.ProcessPolicy,
            ProcessFeed = daemonConfig.ProcessFeed
        };
        return await LoadSegmentAsync(CONFIG_TABLE.GENERIC_CONFIG, defaultValue, stoppingToken);
    }

    public async Task SaveGeneralConfigAsync(GeneralConfig config, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        await UpdateSegmentAsync(config, CONFIG_TABLE.GENERIC_CONFIG, stoppingToken);
    }


    public async Task<WorkItemTempates> GetWorkItemTemplatesAsync(CancellationToken cancellationToken)
    {
        var feedTemplate = await LoadSegmentAsync(CONFIG_TABLE.KIND_WI_FEED, WorkItemTemplate.DEFAULT, cancellationToken);
        var healthTemplate = await LoadSegmentAsync(CONFIG_TABLE.KIND_WI_HEALTH, WorkItemTemplate.DEFAULT, cancellationToken);
        var policyTemplate = await LoadSegmentAsync(CONFIG_TABLE.KIND_WI_POLICY, WorkItemTemplate.DEFAULT, cancellationToken);

        return new WorkItemTempates 
        {
            FeedWorkItemTemplate = feedTemplate,
            PolicyWorkItemTemplate = policyTemplate,
            ServiceHealthWorkItemTemplate = healthTemplate
        };
    }

    public async Task SaveWorkItemTemplateAsync(WorkItemTempates templates, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(templates, nameof(templates));
        ArgumentNullException.ThrowIfNull(templates.FeedWorkItemTemplate, nameof(templates.FeedWorkItemTemplate));
        ArgumentNullException.ThrowIfNull(templates.ServiceHealthWorkItemTemplate, nameof(templates.ServiceHealthWorkItemTemplate));
        ArgumentNullException.ThrowIfNull(templates.PolicyWorkItemTemplate, nameof(templates.PolicyWorkItemTemplate));

        await UpdateSegmentAsync(templates.FeedWorkItemTemplate, CONFIG_TABLE.KIND_WI_FEED, stoppingToken);
        await UpdateSegmentAsync(templates.ServiceHealthWorkItemTemplate, CONFIG_TABLE.KIND_WI_HEALTH, stoppingToken);
        await UpdateSegmentAsync(templates.PolicyWorkItemTemplate, CONFIG_TABLE.KIND_WI_POLICY, stoppingToken);
    }

    public async Task SaveServiceHealthConfigAsync(ServiceHealthConfig config, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var (partitionKey, rowKey) = new Tuple<string, string>(CONFIG_TABLE.PARTITION_KEY, CONFIG_TABLE.ROW_KEY_SVC_HEALTH);

        var serializedConfig = JsonSerializer.Serialize(config);

        await TableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
                {
                    { CONFIG_TABLE.DATA_COLUM, serializedConfig }
                }, TableUpdateMode.Replace, stoppingToken);
    }

    public async Task<ServiceHealthConfig> LoadServiceHealthConfigAsync(CancellationToken stoppingToken)
    {
        try
        {
            var (partitionKey, rowKey) = new Tuple<string, string>(CONFIG_TABLE.PARTITION_KEY, CONFIG_TABLE.ROW_KEY_SVC_HEALTH);
            var existingEntity = await TableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            if (existingEntity != null && existingEntity.HasValue && existingEntity.Value != null)
            {
                var rawData = existingEntity.Value[CONFIG_TABLE.DATA_COLUM];
                var config = JsonSerializer.Deserialize<ServiceHealthConfig>($"{rawData}");
                if (config != null)
                {
                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load config from storage");
        }
        var defaultValue = new ServiceHealthConfig
        {
            Uri = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2021-03-01",
            Subscriptions = ["12a2bcdd-bee4-4c86-9b58-175e1bb43842"],
            KustoQuery =
"""
servicehealthresources
| where type == "microsoft.resourcehealth/events" 
| extend eventType = tostring(properties.EventType)
| where eventType in ('HealthAdvisory', 'SecurityAdvisory')
| extend lastUpdate =  todatetime(tolong(properties.LastUpdateTime))
| where lastUpdate > ago(28d)
// try to replace HTML tags in the Summary by empty string
| extend temp1 = replace('\\<[\\w/]+\\>', '', tostring(properties.Summary))
| extend summary = replace('\\<[\\w/]+\\>', '', temp1)
| extend service = tostring(properties.Impact[0].ImpactedService)
| extend title = tostring(properties.Title)
| extend url = strcat('https://app.azure.com/h/', name)
| summarize arg_max (lastUpdate, *) by name
| project lastUpdate, eventType, name, service, ['title'], summary, url
// most recent update at the bottom, so we can paste new records to the end of an XLS
| sort by lastUpdate asc
"""
        };
        return defaultValue;
    }

    public async Task<AreaPathMapConfig> LoadConfigAsync(CancellationToken stoppingToken)
    {
        var defaultValue = new AreaPathServiceMapConfig(new List<AreaPathServiceMap>(), string.Empty, false);

        var feedConfig = await LoadSegmentAsync<AreaPathServiceMapConfig>(CONFIG_TABLE.KIND_FEED, defaultValue, stoppingToken);
        var serviceHealthConfig = await LoadSegmentAsync<AreaPathServiceMapConfig>(CONFIG_TABLE.KIND_HEALTH, defaultValue, stoppingToken);
        var policyConfig = await LoadSegmentAsync<AreaPathServiceMapConfig>(CONFIG_TABLE.KIND_POLICY, defaultValue, stoppingToken);

        return new AreaPathMapConfig(serviceHealthConfig, policyConfig, feedConfig);
    }

    public async Task SaveConfigAsync(AreaPathMapConfig config, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        await UpdateSegmentAsync(config.AzureUpdatesMap, CONFIG_TABLE.KIND_FEED, stoppingToken);
        await UpdateSegmentAsync(config.ServiceHealthMap, CONFIG_TABLE.KIND_HEALTH, stoppingToken);
        await UpdateSegmentAsync(config.PolicyMap, CONFIG_TABLE.KIND_POLICY, stoppingToken);
    }

    private async Task UpdateSegmentAsync(object config, string configKind, CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var (partitionKey, rowKey) = new Tuple<string, string>(CONFIG_TABLE.PARTITION_KEY, configKind);

        var serializedConfig = JsonSerializer.Serialize(config);

        await TableClient.UpsertEntityAsync(new TableEntity(partitionKey, rowKey)
                {
                    { CONFIG_TABLE.DATA_COLUM, serializedConfig }
                }, TableUpdateMode.Replace, stoppingToken);
    }

    private async Task<TPayload> LoadSegmentAsync<TPayload>(string configKind, TPayload defaultValue, CancellationToken stoppingToken) where TPayload : class
    {
        try
        {
            var (partitionKey, rowKey) = new Tuple<string, string>(CONFIG_TABLE.PARTITION_KEY, configKind);
            var existingEntity = await TableClient.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, null, stoppingToken);
            if (existingEntity != null && existingEntity.HasValue && existingEntity.Value != null)
            {
                var rawData = existingEntity.Value[CONFIG_TABLE.DATA_COLUM];
                var config = JsonSerializer.Deserialize<TPayload>($"{rawData}");
                if (config != null)
                {
                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load config from storage");
        }
        return defaultValue;            
    }



    private class CONFIG_TABLE 
    {
        public const string PARTITION_KEY = "AzureLCM";
        public const string ROW_KEY_SVC_HEALTH = "ServiceHealth";
        public const string DATA_COLUM = "data";

        public const string KIND_FEED = "FeedConfig";
        public const string KIND_HEALTH = "HealthConfig";
        public const string KIND_POLICY = "PolicyConfig";

        public const string KIND_WI_FEED = "Feed_WI_Config";
        public const string KIND_WI_HEALTH = "Health_WI_Config";
        public const string KIND_WI_POLICY = "Policy_WI_Config";

        public const string GENERIC_CONFIG = "GenericConfig";
    }
}
