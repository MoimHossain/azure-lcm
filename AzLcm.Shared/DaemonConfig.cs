

namespace AzLcm.Shared
{
    public class DaemonConfig
    {
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

        public string StorageConnectionString => ReadEnvironmentKey("AZURE_STORAGE_CONNECTION");
        public string FeedTableName => ReadEnvironmentKey("AZURE_STORAGE_FEED_TABLE_NAME");
        public string PolicyTableName => ReadEnvironmentKey("AZURE_STORAGE_POLICY_TABLE_NAME");
        public string AzureOpenAIUrl => ReadEnvironmentKey("AZURE_OPENAI_ENDPOINT");
        public string AzureOpenAIKey => ReadEnvironmentKey("AZURE_OPENAI_API_KEY");
        public string AzureOpenAIGPTDeploymentId => ReadEnvironmentKey("AZURE_OPENAI_GPT_DEPLOYMENT_ID");
        public string AzureOpenAIDavinciDeploymentId => ReadEnvironmentKey("AZURE_OPENAI_DAVINCI_DEPLOYMENT_ID");
        public string AzureUpdateFeedUri => ReadEnvironmentKey("AZURE_UPDATE_FEED_URI");
        public string AzurePolicyGitHubBaseURI => ReadEnvironmentKey("AZURE_POLICY_URI_BASE");
        public string AzurePolicyPath => ReadEnvironmentKey("AZURE_POLICY_PATH");
        public string GitHubPAT => GetEnvironmentVariableAsString("GITHUB_PAT", string.Empty);

        public bool ProcessPolicy => GetEnvironmentVariableAsBool("PROCESS_AZURE_POLICY", false);
        public bool ProcessFeed => GetEnvironmentVariableAsBool("PROCESS_AZURE_FEED", false);

        public string FeedTemplateUri => GetEnvironmentVariableAsString("FEED_WORKITEM_TEMPLATE_URI", string.Empty);
        public string PolicyTemplateUri => GetEnvironmentVariableAsString("POLICY_WORKITEM_TEMPLATE_URI", string.Empty);

        public string FeedPromptTemplateUri => GetEnvironmentVariableAsString("FEED_PROMPT_TEMPLATE_URI", string.Empty);

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
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Environment variable {key} is not set");
            }
            return value;
        }
    }

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


