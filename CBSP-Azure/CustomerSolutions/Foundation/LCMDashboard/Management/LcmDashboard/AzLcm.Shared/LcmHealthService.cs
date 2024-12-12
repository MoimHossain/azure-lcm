
using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.Storage;
using Microsoft.Extensions.Logging;

namespace AzLcm.Shared
{
    public class LcmHealthService(
        DaemonConfig config,        
        FeedStorage feedStorage,
        PolicyStorage policyStorage,
        HealthServiceEventStorage healthServiceEventStorage,
        DevOpsClient devOpsClient,
        CognitiveService cognitiveService,
        ILogger<LcmHealthService> logger)
    {
        public async Task<bool> IsAllServicesUpAndHealthyAsync(CancellationToken cancellationToken)
        {
            var healths = await CheckAppConfigAsync(cancellationToken);

            return healths.All(h => h.IsHealthy);
        }

        public async  Task<List<ServiceDependencyTestResult>> CheckAppConfigAsync(CancellationToken cancellationToken)
        {
            var azdoResult = await CheckAzureDevOpsConnectionAsync(cancellationToken);
            var storageResult = await CheckStorageAccess(cancellationToken);
            var keyVaultResult = await CheckKeyVaultAccessAsync(cancellationToken);

            return [azdoResult, storageResult, keyVaultResult];
        }

        private async Task<ServiceDependencyTestResult> CheckKeyVaultAccessAsync(CancellationToken cancellationToken)
        {
            try
            {
                var (openAIEndpoint, openAIKey, readSucceeded) = await cognitiveService.GetOpenAIConfigFromKeyVaultAsync(cancellationToken);

                if (!readSucceeded)
                {
                    return new ServiceDependencyTestResult("KeyVault", false, "Failed to read OpenAI config from KeyVault.");
                }
                else
                {
                    return new ServiceDependencyTestResult("KeyVault", true, "Connection is healthy.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to check KeyVault connection.");
                return new ServiceDependencyTestResult("KeyVault", false, ex.Message);
            }
        }

        private async Task<ServiceDependencyTestResult> CheckStorageAccess(CancellationToken cancellationToken)
        {
            try
            {
                await feedStorage.EnsureTableExistsAsync(cancellationToken);
                await policyStorage.EnsureTableExistsAsync(cancellationToken);
                await healthServiceEventStorage.EnsureTableExistsAsync(cancellationToken);
                return new ServiceDependencyTestResult("Storage", true, "Connection is healthy.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to check Storage connection.");
                return new ServiceDependencyTestResult("Storage", false, ex.Message);
            }            
        }

        private async Task<ServiceDependencyTestResult> CheckAzureDevOpsConnectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var azdoConfig = config.GetAzureDevOpsClientConfig();
                var connectionData = await devOpsClient.GetConnectionDataAsync(azdoConfig.orgName, cancellationToken);

                if (connectionData.AuthenticatedUser != null &&
                    string.IsNullOrWhiteSpace(connectionData.AuthenticatedUser.SubjectDescriptor))
                {
                    return new ServiceDependencyTestResult("Azure DevOps", true, "Connection is healthy.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to check Azure DevOps connection.");
                return new ServiceDependencyTestResult("Azure DevOps", false, ex.Message);
            }
            return new ServiceDependencyTestResult("Azure DevOps", false, "Connection is unhealthy (Most likely PAT is incorrect).");
        }
    }

    public record ServiceDependencyTestResult(string ServiceName, bool IsHealthy, string Message);
}
