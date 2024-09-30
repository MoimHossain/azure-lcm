


using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.AzureDevOps.Authorizations;
using AzLcm.Shared.AzureDevOps.Authorizations.AuthSchemes;
using AzLcm.Shared.AzureUpdates;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy;
using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

namespace AzLcm.Shared
{
    public static class SharedExtensions
    {
        public static IServiceCollection AddAzureDevOpsClientServices(this IServiceCollection services)
        {   
            services.AddSingleton((services) => {
                var config = services.GetRequiredService<DaemonConfig>();
                return config.GetAzureDevOpsClientConfig();
            });

            services.AddSingleton<PersonalAccessTokenSupport>();
            services.AddSingleton<ServicePrincipalTokenSupport>();
            services.AddSingleton<ManagedIdentityTokenSupport>();
            services.AddHttpClient(AzureDevOpsClientConstants.CoreAPI.NAME,
                (services, client) => { client.BaseAddress = new Uri(AzureDevOpsClientConstants.CoreAPI.URI); });
            services.AddHttpClient(AzureDevOpsClientConstants.VSSPS_API.NAME,
                (services, client) => { client.BaseAddress = new Uri(AzureDevOpsClientConstants.VSSPS_API.URI); });

            // NOTE: transient services
            services.AddTransient<AuthorizationFactory>();
            services.AddTransient<DevOpsClient>();
            return services;
        }

        public async static Task<(Uri, string, bool)> GetOpenAIConfigFromKeyVaultAsync(DaemonConfig config)
        {
            try
            {
                if (config != null && !string.IsNullOrWhiteSpace(config.KeyVaultURI))
                {
                    SecretClientOptions options = new()
                    {
                        Retry =
                        {
                             Delay= TimeSpan.FromSeconds(2),
                             MaxDelay = TimeSpan.FromSeconds(16),
                             MaxRetries = 5,
                             Mode = RetryMode.Exponential
                        }
                    };
                    var kvClient = new SecretClient(new Uri(config.KeyVaultURI), new DefaultAzureCredential(), options);
                    KeyVaultSecret openAIEndpoint = await kvClient.GetSecretAsync("AOIEndpoint");
                    KeyVaultSecret openAiKey = await kvClient.GetSecretAsync("AOIKey");
                    if (openAIEndpoint != null)
                    {
                        return new(new Uri(openAIEndpoint.Value), openAiKey.Value, true);
                    }
                }
            }
            catch (Exception ex)
            {
                // log the exception
                Console.WriteLine("Failed to read the key vault");
                Console.WriteLine(ex.Message);
            }
            return new(new Uri("https://microsoft.com"), string.Empty, false);
        }

        public static IServiceCollection AddRequiredServices(this IServiceCollection services)
        {   
            services.AddSingleton<DaemonConfig>();
            services.AddSingleton<WorkItemTemplateStorage>();
            services.AddSingleton<PromptTemplateStorage>();
            services.AddSingleton<BlobContentReader>();
            services.AddSingleton<FeedStorage>();
            services.AddSingleton<PolicyStorage>();
            services.AddSingleton<HealthServiceEventStorage>();
            services.AddSingleton<PolicyReader>();            
            services.AddSingleton<AzUpdateSyndicationFeed>();
            services.AddSingleton<ServiceHealthConfigReader>();
            services.AddSingleton<ServiceHealthReader>();
            services.AddSingleton<HtmlExtractor>();

            services.AddSingleton(async (services) => {
                var config = services.GetRequiredService<DaemonConfig>();
                // From now, the open AI config should be read from Key vault with a managed identity
                // assuming the key vault and open AI both are accessed via Private endpoints
                var (openAIEndpoint, openAIKey, readSucceeded ) = await GetOpenAIConfigFromKeyVaultAsync(config);
                return new OpenAIClient(openAIEndpoint, new AzureKeyCredential(openAIKey));
            });
            services.AddSingleton<CognitiveService>();
            services.AddAzureDevOpsClientServices();
            return services;
        }
    }
}
