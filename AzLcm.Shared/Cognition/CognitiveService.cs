

using AzLcm.Shared.Cognition.Models;
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

        public async Task<Verdict?> AnalyzeAsync(SyndicationItem feed)
        {
            ArgumentNullException.ThrowIfNull(nameof(feed));

            var thread = GetChatCompletionsOptions();

            thread.Messages.Add(new ChatRequestSystemMessage(
                """
                You are Product Owner for the Life Cycle management team. 
                You need to classify the Azure Updates:
                    1. If some service is retired, unsupported or deprecated.
                        a. If possible we should also come up with azure policy to mitigate the impact.
                    2. If some service became GA or Preview, we must make announcement.
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
            thread.Messages.Add(new ChatRequestUserMessage(feedDetails.ToString()));

            try
            {
                var response = await openAIClient.GetChatCompletionsAsync(thread);
                var rawContent = response.Value.Choices[0].Message.Content;

                logger.LogInformation($"ASK: {feed.Title.Text}");
                logger.LogInformation($"Response: {rawContent}");

                var verdict = Verdict.FromJson(rawContent, jsonSerializerOptions);
                if (verdict != null)
                {
                    var message = new StringBuilder();
                    message.AppendLine($"==============================================================");
                    message.AppendLine($"Title: {feed.Title.Text}");
                    message.AppendLine($"");
                    if(verdict.AzureServiceNames != null )
                    {
                        message.AppendLine($"AzureServiceNames: {string.Join(", ", verdict.AzureServiceNames)}");
                    }                    
                    message.AppendLine($"Update Kind: {verdict.UpdateKind}");
                    message.AppendLine($"Actionable: {verdict.Actionable}");
                    message.AppendLine($"ActionableViaAzurePolicy: {verdict.ActionableViaAzurePolicy}");
                    message.AppendLine($"AnnouncementRequired: {verdict.AnnouncementRequired}");
                    message.AppendLine($"MitigationInstructionMarkdown: {verdict.MitigationInstructionMarkdown}");
                    message.AppendLine($"");
                    message.AppendLine($"");


                    logger.LogInformation(message.ToString());

                    //File.AppendAllText(@"C:\\GitHub\\moimhossain\\azure-lcm\\traces.txt", message.ToString());
                }
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

