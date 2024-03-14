

using Azure.AI.OpenAI;
using System.ServiceModel.Syndication;
using System.Text;

namespace AzLcm.Shared.Cognition
{
    public class CognitiveService(
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

        public async Task AnalyzeAsync(SyndicationItem feed)
        {
            ArgumentNullException.ThrowIfNull(nameof(feed));

            var thread = GetChatCompletionsOptions();

            thread.Messages.Add(new ChatRequestSystemMessage(
                """
                You are Product Owner for the Life Cycle management team. 
                You need to classify the Azure Updates:
                    1. If some service is retired, unsupported or deprecated,
                    2. If some service became GA or Preview
                
                You MUST Produce response in JSON that follows the schema below:                
                ```
                type {
                    azureServiceName: string,
                    updateKind: 'Retired' | 'Deprecated' | 'Unsupported' | 'GenerallyAvailable' | 'Preview',
                    actionable: boolean
                    announcementRequired: boolean
                    actionableViaAzurePolicy: boolean
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

            var response = await openAIClient.GetChatCompletionsAsync(thread);
            
            var s = response.Value.Choices[0].Message.Content;
        }
    }
}

