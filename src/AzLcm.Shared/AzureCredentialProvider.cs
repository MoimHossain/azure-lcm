

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace AzLcm.Shared
{
    public class AzureCredentialProvider(
        DaemonConfig config,
        ILogger<AzureCredentialProvider> logger)
    {
        private TokenCredential GetCommonCredential()
        {
            if (!string.IsNullOrWhiteSpace(config.UserAssignedManagedIdentityClientId))
            {
                return new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = config.UserAssignedManagedIdentityClientId
                    });
            }
            return new DefaultAzureCredential();
        }

        public TokenCredential GetKVCredential()
        {
            return GetCommonCredential();
        }

        public TokenCredential GetStorageCredential()
        {
            return GetCommonCredential();
        }

        public TokenCredential GetSvcHealthQueryCredential()
        {
            var azConfig = config.GetAzureConnectionConfig();
            TokenCredential? cred;
            if (!string.IsNullOrWhiteSpace(azConfig.ClientId) && !string.IsNullOrWhiteSpace(azConfig.ClientSecret))
            {
                logger.LogInformation("Azure Service Health Query:: Using client secret for authentication");
                cred = new ClientSecretCredential(azConfig.TenantId, azConfig.ClientId, azConfig.ClientSecret);
            }
            else
            {
                logger.LogInformation("Azure Service Health Query:: Using default Azure credential for authentication");
                cred = GetCommonCredential();
            }
            return cred;
        }
    }
}
