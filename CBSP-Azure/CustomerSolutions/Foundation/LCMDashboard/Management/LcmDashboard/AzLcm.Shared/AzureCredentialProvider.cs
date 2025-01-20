

using Azure.Core;
using Azure.Identity;

namespace AzLcm.Shared
{
    public class AzureCredentialProvider(DaemonConfig config)
    {
        public TokenCredential GetCredentail()
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
    }
}
