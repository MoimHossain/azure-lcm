


using AzLcm.Shared.AzureDevOps.Authorizations.AuthSchemes;


namespace AzLcm.Shared.AzureDevOps.Authorizations
{
    // Transient in DI
    public class AuthorizationFactory
    {        
        private readonly ServicePrincipalTokenSupport servicePrincipalTokenSupport;
        private readonly PersonalAccessTokenSupport personalAccessTokenSupport;
        private readonly ManagedIdentityTokenSupport managedIdentityTokenSupport;
        private readonly AzureDevOpsClientConfig config;

        public AuthorizationFactory(            
            ServicePrincipalTokenSupport servicePrincipalTokenSupport,
            PersonalAccessTokenSupport personalAccessTokenSupport,
            ManagedIdentityTokenSupport managedIdentityTokenSupport,
            AzureDevOpsClientConfig config)
        {
            this.servicePrincipalTokenSupport = servicePrincipalTokenSupport;
            this.personalAccessTokenSupport = personalAccessTokenSupport;
            this.managedIdentityTokenSupport = managedIdentityTokenSupport;
            this.config = config;
        }

        public async Task<(string, string)> GetCredentialsAsync(bool elevatedPrivilege = false)
        {
            if (managedIdentityTokenSupport.IsConfigured())
            {
                return await managedIdentityTokenSupport.GetCredentialsAsync();
            }
            if (servicePrincipalTokenSupport.IsConfigured())
            {
                return await servicePrincipalTokenSupport.GetCredentialsAsync();
            }
            if (personalAccessTokenSupport.IsConfigured())
            {
                return personalAccessTokenSupport.GetCredentials();
            }
            throw new InvalidOperationException("Please configure Azure DevOps authorization scheme in Env variables.");
        }
    }
}
