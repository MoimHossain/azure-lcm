

namespace AzLcm.Shared.Storage
{
    public class PromptTemplateStorage(DaemonConfig daemonConfig, IHttpClientFactory httpClientFactory)
    {
        public async Task<string> GetFeedPromptAsync(CancellationToken stoppingToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(daemonConfig.FeedPromptTemplateUri, 
                nameof(daemonConfig.FeedPromptTemplateUri));

            var httpClient = httpClientFactory.CreateClient();
            var prompt = await httpClient.GetStringAsync(daemonConfig.FeedPromptTemplateUri, stoppingToken);
            return prompt;
        }
    }
}
