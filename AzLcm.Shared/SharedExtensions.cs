


using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.AzureDevOps.Authorizations;
using AzLcm.Shared.AzureDevOps.Authorizations.AuthSchemes;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy;
using AzLcm.Shared.Storage;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;

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

        public static IServiceCollection AddRequiredServices(this IServiceCollection services)
        {   
            services.AddSingleton<DaemonConfig>();
            services.AddSingleton<WorkItemTemplateStorage>();
            services.AddSingleton<PromptTemplateStorage>();
            services.AddSingleton<FeedStorage>();
            services.AddSingleton<PolicyStorage>();
            services.AddSingleton<PolicyReader>();
            services.AddSingleton<AzUpdateSyndicationFeed>();
            services.AddSingleton<HtmlExtractor>();

            services.AddSingleton((services) => {
                var config = services.GetRequiredService<DaemonConfig>();
                return new OpenAIClient(new Uri(config.AzureOpenAIUrl), new AzureKeyCredential(config.AzureOpenAIKey));
            });
            services.AddSingleton<CognitiveService>();
            services.AddAzureDevOpsClientServices();
            return services;
        }
    }
}
