

using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AzLcm.Shared.ServiceHealth
{
    public class ServiceHealthReader(
        ILogger<ServiceHealthReader> logger,
        DaemonConfig daemonConfig,
        ServiceHealthConfigReader serviceHealthConfigReader,
        IHttpClientFactory httpClientFactory)
    {
        public async Task ProcessAsync(Func<ServiceHealthEvent, Task> work, CancellationToken cancellationToken)
        {
            var config = daemonConfig.GetAzureConnectionConfig();            
            var healthConfig = await serviceHealthConfigReader.GetResourceGraphQueryConfig(cancellationToken);
            if(healthConfig == null )
            {
                logger.LogWarning("No health configuration found");
                return;
            }
            var scConfig = await serviceHealthConfigReader.GetResourceGraphQueryConfig(cancellationToken);
            var queryText = scConfig?.KustoQuery;
            var resourceGraphUri = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2021-03-01";
            if(!string.IsNullOrWhiteSpace(healthConfig.Uri))
            {
                resourceGraphUri = healthConfig.Uri;
            }

            logger.LogInformation("Querying Azure Resource Graph with query: {Query}", queryText);

            TokenCredential? cred;
            if (!string.IsNullOrWhiteSpace(config.ClientId) && !string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                logger.LogInformation("Azure Service Health Query:: Using client secret for authentication");
                cred = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
            }
            else 
            {
                logger.LogInformation("Azure Service Health Query:: Using default Azure credential for authentication");
                cred = new DefaultAzureCredential();
            }            
            
            var accessToken = await cred.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/.default"]), cancellationToken);
            logger.LogInformation("Azure Service Health Query:: Access token acquired for {clientId}", config.ClientId);

            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);            
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(resourceGraphUri);
                        
            var response = await httpClient
                .PostAsync(resourceGraphUri, 
                new StringContent(JsonSerializer.Serialize(new
                {
                    subscriptions = healthConfig.Subscriptions,
                    query = queryText
                }), Encoding.UTF8, "application/json"), cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Azure Service Health Query:: Successfully queried Azure Resource Graph with status code {StatusCode}", response.StatusCode);
                var content = await response.Content.ReadFromJsonAsync<ServiceHealthCollection>(cancellationToken);

                if (content != null)
                {
                    logger.LogInformation("Azure Service Health Query::Received {Count} health items", content.Count);
                    foreach (var item in content.Events)
                    {
                        logger.LogInformation("Azure Service Health Query::Health item: {Item}", item.Name);
                        await work(item);
                    }
                }
                else
                {
                    logger.LogWarning("Azure Service Health Query::No health items received");
                }
            }
            else
            {
                logger.LogError("Azure Service Health Query::Failed to query Azure Resource Graph with status code {StatusCode}", response.StatusCode);
            }
        }
    }
}
