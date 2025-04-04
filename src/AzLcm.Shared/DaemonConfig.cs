﻿

namespace AzLcm.Shared
{
    public class DaemonConfig
    {
        public AzureConnectionConfig GetAzureConnectionConfig()
        {
            var tenantId = GetEnvironmentVariableAsString("AZURE_TENANT_ID", "");
            var clientId = GetEnvironmentVariableAsString("AZURE_CLIENT_ID", "");
            var clientSecret = GetEnvironmentVariableAsString("AZURE_CLIENT_SECRET", "");

            return new AzureConnectionConfig(tenantId, clientId, clientSecret);
        }

        public AzureDevOpsClientConfig GetAzureDevOpsClientConfig()
        {
            var orgName = GetEnvironmentVariableAsString("AZURE_DEVOPS_ORGNAME", "");
            var useManagedIdentity = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_MANAGED_IDENTITY", false);
            var clientIdOfManagedIdentity = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_ID_OF_MANAGED_IDENTITY", "");
            var tenantIdOfManagedIdentity = GetEnvironmentVariableAsString("AZURE_DEVOPS_TENANT_ID_OF_MANAGED_IDENTITY", "");

            var useServicePrincipal = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_SERVICE_PRINCIPAL", false);
            var clientIdOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_ID_OF_SERVICE_PRINCIPAL", "");
            var clientSecretOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_CLIENT_SECRET_OF_SERVICE_PRINCIPAL", "");
            var tenantIdOfServicePrincipal = GetEnvironmentVariableAsString("AZURE_DEVOPS_TENANT_ID_OF_SERVICE_PRINCIPAL", "");

            var usePat = GetEnvironmentVariableAsBool("AZURE_DEVOPS_USE_PAT", false);
            var pat = GetEnvironmentVariableAsString("AZURE_DEVOPS_PAT", "");

            return new AzureDevOpsClientConfig(
                orgName,
                useManagedIdentity, clientIdOfManagedIdentity, tenantIdOfManagedIdentity,
                useServicePrincipal, clientIdOfServicePrincipal, clientSecretOfServicePrincipal, tenantIdOfServicePrincipal,
                usePat, pat);
        }

        public static string AppInsightConnectionString => ReadEnvironmentKey("APPLICATION_INSIGHT_CONNECTION_STRING");

        public string ConfigTableName => "configs";
        public string UserAssignedManagedIdentityClientId => ReadEnvironmentKey("USER_ASSIGNED_MANAGED_IDENTITY_CLIENT_ID");
        public string StorageAccountName => ReadEnvironmentKey("AZURE_STORAGE_ACCOUNT_NAME");
        public string FeedTableName => ReadEnvironmentKey("AZURE_STORAGE_FEED_TABLE_NAME");
        public string PolicyTableName => ReadEnvironmentKey("AZURE_STORAGE_POLICY_TABLE_NAME");
        public string ServiceHealthTableName => ReadEnvironmentKey("AZURE_STORAGE_SVC_HEALTH_TABLE_NAME");
        public string StorageConfigContainer => ReadEnvironmentKey("AZURE_STORAGE_COFIG_CONTAINER");
        public string KeyVaultURI = ReadEnvironmentKey("AZURE_KEY_VAULT_URI");
        public string AzureOpenAIUrl => ReadEnvironmentKey("AZURE_OPENAI_ENDPOINT");
        public string AzureOpenAIKey => ReadEnvironmentKey("AZURE_OPENAI_API_KEY");
        public string AzureOpenAIGPTDeploymentId => ReadEnvironmentKey("AZURE_OPENAI_GPT_DEPLOYMENT_ID");
        
        public string AzureUpdateFeedUri => ReadEnvironmentKey("AZURE_UPDATE_FEED_URI");
        public string AzurePolicyGitHubBaseURI => ReadEnvironmentKey("AZURE_POLICY_URI_BASE");
        public string AzurePolicyPath => ReadEnvironmentKey("AZURE_POLICY_PATH");
        public string GitHubPAT => GetEnvironmentVariableAsString("GITHUB_PAT", string.Empty);

        public bool ProcessServiceHealth => GetEnvironmentVariableAsBool("PROCESS_AZURE_SERVICE_HEALTH", false);
        public bool ProcessPolicy => GetEnvironmentVariableAsBool("PROCESS_AZURE_POLICY", false);
        public bool ProcessFeed => GetEnvironmentVariableAsBool("PROCESS_AZURE_FEED", false);


        private static string GetEnvironmentVariableAsString(string name, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static bool GetEnvironmentVariableAsBool(string name, bool defaultValue)
        {
            var value = ReadEnvironmentKey(name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.Parse(value);
        }

        private static string ReadEnvironmentKey(string key)
        {
            var value = string.Empty;
            try 
            {
                value = Environment.GetEnvironmentVariable(key);
            }
            catch 
            {
              
            }
           
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }
            return value;
        }
    }

    public record AzureConnectionConfig(string TenantId, string ClientId, string ClientSecret);

    public record AzureDevOpsClientConfig(
        string orgName,
        bool useManagedIdentity, string clientIdOfManagedIdentity, string tenantIdOfManagedIdentity,
        bool useServicePrincipal, string clientIdOfServicePrincipal, string clientSecretOfServicePrincipal, string tenantIdOfServicePrincipal,
        bool usePat, string Pat);

    public static class AzureDevOpsClientConstants
    {
        public static class CoreAPI
        {
            public const string NAME = "AZUREDEVOPS_CORE_CLIENT";
            public const string URI = "https://dev.azure.com";
        }

        public static class VSSPS_API
        {
            public const string NAME = "AZUREDEVOPS_VSSPS_CLIENT";
            public const string URI = "https://vssps.dev.azure.com";
        }

    }
}


