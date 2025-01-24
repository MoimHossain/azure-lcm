

using Azure.Core;
using Azure.Identity;

namespace AzLcm.Shared
{
    public class AzureCredentialProvider(DaemonConfig config)
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
    }
}
