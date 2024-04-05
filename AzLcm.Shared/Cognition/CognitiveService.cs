

using AzLcm.Shared.Cognition.Models;
using AzLcm.Shared.PageScrapping;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json;

namespace AzLcm.Shared.Cognition
{
    public class CognitiveService(
        JsonSerializerOptions jsonSerializerOptions,
        ILogger<CognitiveService> logger,
        DaemonConfig daemonConfig,
        OpenAIClient openAIClient)
    {
        private ChatCompletionsOptions GetChatCompletionsOptions() => new()
        {
            DeploymentName = daemonConfig.AzureOpenAIGPTDeploymentId,
            ChoiceCount = 1,            
            MaxTokens = 4000,
            FrequencyPenalty = (float)0,
            PresencePenalty = (float)0,
            Temperature = (float)1
        };

        public async Task<Verdict?> AnalyzeAsync(
            SyndicationItem feed, List<HtmlFragment> fragments, 
            string promptTemplate, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(feed));

            var thread = GetChatCompletionsOptions();

            thread.Messages.Add(new ChatRequestSystemMessage(promptTemplate));            

            var feedDetails = new StringBuilder();            
            feedDetails.AppendLine($"Title: {feed.Title.Text}");
            feedDetails.AppendLine($"Summary: {feed.Summary.Text}");

            var textContent = feed.Content as System.ServiceModel.Syndication.TextSyndicationContent;
            if (textContent != null)
            {
                feedDetails.AppendLine($"Content: {textContent.Text}");
            }

            foreach (var fragment in fragments)
            {
                feedDetails.AppendLine($"{fragment.Content}");
                if(fragment.Links != null)
                {
                    foreach(var link in fragment.Links)
                    {
                        feedDetails.AppendLine($"Ref: {link}");
                    }
                }
            }

            thread.Messages.Add(new ChatRequestUserMessage(feedDetails.ToString()));

            try
            {
                var response = await openAIClient.GetChatCompletionsAsync(thread, stoppingToken);
                var rawContent = response.Value.Choices[0].Message.Content;
                var verdict = Verdict.FromJson(rawContent, jsonSerializerOptions);
                return verdict;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: "");
            }
            return null;
        }
    }
}

