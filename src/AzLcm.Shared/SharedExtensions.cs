﻿


using AzLcm.Shared.AzureDevOps;
using AzLcm.Shared.AzureDevOps.Authorizations;
using AzLcm.Shared.AzureDevOps.Authorizations.AuthSchemes;
using AzLcm.Shared.AzureUpdates;
using AzLcm.Shared.Cognition;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Policy;
using AzLcm.Shared.ServiceHealth;
using AzLcm.Shared.Storage;

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
            services.AddSingleton<AzureCredentialProvider>();
            services.AddSingleton<WorkItemTemplateStorage>();
            services.AddSingleton<PromptTemplateStorage>();            
            services.AddSingleton<ConfigurationStorage>();
            services.AddSingleton<FeedStorage>();
            services.AddSingleton<PolicyStorage>();
            services.AddSingleton<HealthServiceEventStorage>();
            services.AddSingleton<PolicyReader>();            
            services.AddSingleton<AzUpdateSyndicationFeed>();
            services.AddSingleton<ServiceHealthConfigReader>();
            services.AddSingleton<ServiceHealthReader>();
            services.AddSingleton<HtmlExtractor>();
            services.AddSingleton<CognitiveService>();
            services.AddSingleton<LcmHealthService>();
            services.AddAzureDevOpsClientServices();
            return services;
        }
    }
}
