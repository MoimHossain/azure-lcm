


using AzLcm.Shared.Storage;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.DependencyInjection;

namespace AzLcm.Shared
{
    public static class SharedExtensions
    {
        public static IServiceCollection AddRequiredServices(this IServiceCollection services)
        {   
            services.AddSingleton<DaemonConfig>();
            services.AddSingleton<FeedStorage>();
            services.AddSingleton<AzUpdateSyndicationFeed>();

            services.AddSingleton((services) => {
                var config = services.GetRequiredService<DaemonConfig>();
                return new OpenAIClient(new Uri(config.AzureOpenAIUrl), new AzureKeyCredential(config.AzureOpenAIKey));
            });

            return services;
        }
    }
}
