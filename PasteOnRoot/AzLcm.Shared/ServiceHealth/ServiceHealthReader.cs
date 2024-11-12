

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
            var queryText = await serviceHealthConfigReader.GetKustoQueryAsync(cancellationToken);
            var resourceGraphUri = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2021-03-01";
            if(!string.IsNullOrWhiteSpace(healthConfig.Uri))
            {
                resourceGraphUri = healthConfig.Uri;
            }

            logger.LogInformation("Querying Azure Resource Graph with query: {Query}", queryText);

            TokenCredential? cred;
            if (!string.IsNullOrWhiteSpace(config.ClientId) && !string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                logger.LogInformation("Using client secret for authentication");
                cred = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
            }
            else 
            {
                logger.LogInformation("Using default Azure credential for authentication");
                cred = new DefaultAzureCredential();
            }            
            
            var accessToken = await cred.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/.default"]), cancellationToken);
            logger.LogInformation("Access token acquired for {clientId}", config.ClientId);

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
                logger.LogInformation("Successfully queried Azure Resource Graph with status code {StatusCode}", response.StatusCode);
                var content = await response.Content.ReadFromJsonAsync<ServiceHealthCollection>(cancellationToken);

                if (content != null)
                {
                    logger.LogInformation("Received {Count} health items", content.Count);
                    foreach (var item in content.Events)
                    {
                        logger.LogInformation("Health item: {Item}", item.Name);
                        await work(item);
                    }
                }
                else
                {
                    logger.LogWarning("No health items received");
                }
            }
            else
            {
                logger.LogError("Failed to query Azure Resource Graph with status code {StatusCode}", response.StatusCode);
            }
        }
    }
}
