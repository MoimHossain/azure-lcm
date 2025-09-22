


using AzLcm.Shared.Cognition.Models;
using AzLcm.Shared.Logging;
using AzLcm.Shared.Storage;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Text;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace AzLcm.Shared.Cognition
{
    public class CognitiveService(
        AzureCredentialProvider azureCredentialProvider,
        ILogger<CognitiveService> logger,
        DaemonConfig daemonConfig)
    {
        private static ChatOptions GetChatOptions() => new()
        {            
            FrequencyPenalty = (float)0,
            PresencePenalty = (float)0,
            Temperature = (float)1
        };

        public async Task<Verdict?> AnalyzeV2Async(
            SyndicationItem feedItem, string promptTemplate, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(feedItem, nameof(feedItem));
            ArgumentException.ThrowIfNullOrWhiteSpace(promptTemplate, nameof(promptTemplate));

            using var scope = logger.BeginOperationScope("AnalyzeV2Async", 
                new { FeedTitle = feedItem.Title?.Text, FeedId = feedItem.Id });

            try
            {
                logger.LogOperationStart("AnalyzeV2Async", new { FeedTitle = feedItem.Title?.Text });

                List<ChatMessage> messages = [new(ChatRole.System, promptTemplate)];
                var tags = feedItem.Categories.Select(c => c.Name).ToList();
                var feedDetails = new StringBuilder();
                feedDetails.AppendLine($"<Update info BEGIN>");
                feedDetails.AppendLine($"Title: {feedItem.Title?.Text}");
                feedDetails.AppendLine($"Summary: {feedItem.Summary.Text}");
                feedDetails.AppendLine($"Tags: {string.Join(", ", tags)}");
                feedDetails.AppendLine($"</Update info END>");            
                messages.Add(new ChatMessage(ChatRole.User, feedDetails.ToString()));

                var openAIClient = await GetOpenAIClient(stoppingToken);
                var chatClient = openAIClient.AsChatClient(modelId: daemonConfig.AzureOpenAIGPTDeploymentId);

                logger.LogExternalServiceCall("OpenAI", "CompleteAsync", new { ModelId = daemonConfig.AzureOpenAIGPTDeploymentId });

                var response = await chatClient
                    .CompleteAsync<Verdict>(messages, GetChatOptions(), cancellationToken: stoppingToken);
                
                if (response.TryGetResult(out var orchestrationGroundedResponse) && orchestrationGroundedResponse != null)
                {
                    logger.LogOperationSuccess("AnalyzeV2Async", TimeSpan.Zero, new { Verdict = orchestrationGroundedResponse.UpdateKind });
                    return orchestrationGroundedResponse;
                }

                logger.LogOperationSuccess("AnalyzeV2Async", TimeSpan.Zero, new { Result = "No verdict generated" });
                return null;
            }
            catch (Exception ex)
            {
                logger.LogOperationFailure("AnalyzeV2Async", ex, new { FeedTitle = feedItem.Title?.Text });
                throw;
            }
        }

        public async Task<AreaPathMapResponse?> MapServiceToAreaPathAsync(
            string serviceName,
            AreaPathServiceMapConfig areaPathServiceMapConfig, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(areaPathServiceMapConfig));

            var validAreaPaths = areaPathServiceMapConfig.Map.Select(m => m.RouteToAreaPath).ToList();
            validAreaPaths.Add(areaPathServiceMapConfig.DefaultAreaPath);

            List<ChatMessage> messages = [new(ChatRole.System, 
"""                
You have a map of services to area paths. Your task is to map the service name to the area path.
""" 
            )];
            var mapInfoBuilder = new StringBuilder();
            foreach (var mapItem in areaPathServiceMapConfig.Map)
            {
                mapInfoBuilder.AppendLine($"[");
                mapInfoBuilder.AppendLine($" Service: {string.Join(", ", mapItem.Services)}");
                mapInfoBuilder.AppendLine($" AreaPath: {mapItem.RouteToAreaPath}");
                mapInfoBuilder.AppendLine($"] {Environment.NewLine}");
            }
            messages.Add(new ChatMessage(ChatRole.System, mapInfoBuilder.ToString()));
            messages.Add(new ChatMessage(ChatRole.User, $"Service Name: {serviceName}"));

            
            try
            {
                var openAIClient = await GetOpenAIClient(stoppingToken);
                var chatClient = openAIClient.AsChatClient(modelId: daemonConfig.AzureOpenAIGPTDeploymentId);

                var response = await chatClient
                    .CompleteAsync<AreaPathMapResponse>(messages, GetChatOptions(), cancellationToken: stoppingToken);

                if (response.TryGetResult(out var orchestrationGroundedResponse) && orchestrationGroundedResponse != null)
                {
                    var suggestedPath = $"{orchestrationGroundedResponse.AreaPath}";
                    bool contains = validAreaPaths.Any(vap => vap.Contains(suggestedPath, StringComparison.OrdinalIgnoreCase));

                    if (contains)
                    {
                        return orchestrationGroundedResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: $"Error in MapServiceToAreaPathAsync()");
            }
            return new AreaPathMapResponse { AreaPath = string.Empty };
        }


        public async Task<bool> CheckOpenAIAccessAsync(CancellationToken stoppingToken)
        {
            var openAIClient = await GetOpenAIClient(stoppingToken);
            var chatClient = openAIClient.AsChatClient(modelId: daemonConfig.AzureOpenAIGPTDeploymentId);

            List<ChatMessage> messages = [new(ChatRole.System, """Reply with one word only.""")];
            messages.Add(new ChatMessage(ChatRole.User, $"How are you?"));

            try
            {
                var response = await chatClient.CompleteAsync(messages, GetChatOptions(), cancellationToken: stoppingToken);
                logger.LogInformation("OpenAI Access Check Response: {Response}", response.Message.Text);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: "");
            }
            return false;
        }


        public async Task<AzureOpenAIClient> GetOpenAIClient(CancellationToken cancellationToken)
        {
            if (_openAIClient == null)
            {
                var (openAIEndpoint, openAIKey, readSucceeded ) = await GetOpenAIConfigFromKeyVaultAsync(cancellationToken);

                if(!readSucceeded)
                {
                    throw new InvalidOperationException("Failed to read the OpenAI config from KeyVault");
                }
                _openAIClient = new AzureOpenAIClient(openAIEndpoint, new AzureKeyCredential(openAIKey));
            }
            return _openAIClient;
        }

        public async Task<(Uri, string, bool)> GetOpenAIConfigFromKeyVaultAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Reading OpenAI URI and Key from {KeyVaultURI} ", daemonConfig.KeyVaultURI);
                if (daemonConfig != null && !string.IsNullOrWhiteSpace(daemonConfig.KeyVaultURI))
                {
                    SecretClientOptions options = new()
                    {
                        Retry =
                        {
                             Delay= TimeSpan.FromSeconds(2),
                             MaxDelay = TimeSpan.FromSeconds(5),
                             MaxRetries = 2,
                             Mode = RetryMode.Exponential
                        }
                    };
                    var kvClient = new SecretClient(new Uri(daemonConfig.KeyVaultURI), azureCredentialProvider.GetKVCredential(), options);
                    KeyVaultSecret openAIEndpoint = await kvClient.GetSecretAsync("AOIEndpoint", cancellationToken: cancellationToken);
                    KeyVaultSecret openAiKey = await kvClient.GetSecretAsync("AOIKey", cancellationToken: cancellationToken);
                    if (openAIEndpoint != null)
                    {
                        logger.LogInformation("OpenAI URI: {OpenAIEndpoint}", openAIEndpoint.Value);
                        return new(new Uri(openAIEndpoint.Value), openAiKey.Value, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: "Failed to read OpenAI config from KeyVault");
            }
            return new(new Uri("https://microsoft.com"), string.Empty, false);
        }

        private AzureOpenAIClient? _openAIClient = null;
    }
}

