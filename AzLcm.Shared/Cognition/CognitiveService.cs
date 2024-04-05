

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
            SyndicationItem feed, List<HtmlFragment> fragments, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(feed));

            var thread = GetChatCompletionsOptions();

            thread.Messages.Add(new ChatRequestSystemMessage(
                """
                You are Product Owner for the Life Cycle management team. 
                You need to classify the following Azure Update feed:
                    1. If any service is retired, unsupported or deprecated.
                        a. When possible you should come up with azure policy (code snippet) to mitigate the impact.
                    2. If any service became GA or Preview, you must make announcement.
                    3. If you can't classify an update set updateKind to Unknown.
                    4. Only respond with JSON content - never respond with free texts.
                
                You MUST Produce response in JSON that follows the schema below:                
                ```
                type {
                    azureServiceNames: string[]
                    updateKind: 'Retired' | 'Deprecated' | 'Unsupported' | 'GenerallyAvailable' | 'Preview' | 'Unknown',
                    actionable: boolean
                    announcementRequired: boolean
                    actionableViaAzurePolicy: boolean
                    mitigationInstructionMarkdown: string | null
                }
                ```
                Example response:
                ```
                {
                    "azureServiceNames": ["Log Analytics", "Azure Monitor"],
                    "updateKind": "Retired",
                    "actionable": true,
                    "announcementRequired": true,
                    "actionableViaAzurePolicy": true,
                    "mitigationInstructionMarkdown": "You can mitigate further consuming the service with following policy: { ...policy code snippet } "
                }
                ```
                """));
            //thread.Messages.Add(new ChatRequestAssistantMessage(""));
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

