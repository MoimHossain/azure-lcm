

using AzLcm.Shared.AzureUpdates.Model;
using AzLcm.Shared.Cognition.Models;
using AzLcm.Shared.PageScrapping;
using AzLcm.Shared.Storage;
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
        private ChatCompletionsOptions GetChatCompletionsOptions(float temperature = (float)1) => new()
        {
            DeploymentName = daemonConfig.AzureOpenAIGPTDeploymentId,
            ChoiceCount = 1,            
            MaxTokens = 4000,
            FrequencyPenalty = (float)0,
            PresencePenalty = (float)0,
            Temperature = (float)temperature
        };

        public async Task<Verdict?> AnalyzeV2Async(
            AzFeedItem feedItem,  string promptTemplate, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(feedItem));

            var thread = GetChatCompletionsOptions();

            thread.Messages.Add(new ChatRequestSystemMessage(promptTemplate));

            var feedDetails = new StringBuilder();
            feedDetails.AppendLine($"Title: {feedItem.Title}");
            feedDetails.AppendLine($"Summary: {feedItem.UpdateBody}");
            feedDetails.AppendLine($"Tags: {string.Join(", ", feedItem.Tags)}");
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

        public async Task<AreaPathMapResponse?> MapServiceToAreaPathAsync(
            string serviceName,
            AreaPathServiceMapConfig areaPathServiceMapConfig, CancellationToken stoppingToken)
        {
            ArgumentNullException.ThrowIfNull(nameof(areaPathServiceMapConfig));

            var thread = GetChatCompletionsOptions((float)0.9);
            thread.Messages.Add(new ChatRequestSystemMessage(
"""
You have a map of services to area paths. Your task is to map the service name to the area path.
"""));
            
            var mapInfoBuilder = new StringBuilder();
            foreach(var mapItem in areaPathServiceMapConfig.Map)
            {
                mapInfoBuilder.AppendLine($"[");
                mapInfoBuilder.AppendLine($" Service: {string.Join(", ", mapItem.Services)}");
                mapInfoBuilder.AppendLine($" AreaPath: {mapItem.RouteToAreaPath}");
                mapInfoBuilder.AppendLine($"] {Environment.NewLine}");
            }
            thread.Messages.Add(new ChatRequestSystemMessage(mapInfoBuilder.ToString()));

            thread.Messages.Add(new ChatRequestSystemMessage(
"""
IMPORTANT: You must response with JSON object with following schema:

export type MapResponse {
    areaPath: string;
}

Example1: If user provide a service name "Azure SQL" and it matches to an area path '/area-path/demo'
Response should be:
    { areaPath: "/area-path/demo", matched: true }

Example2: If user provide a service name "Azure X-Service" and there is no match found
Response should be:
    { areaPath: "", matched: false }

The response MUST be in JSON format!!
"""));


            thread.Messages.Add(new ChatRequestUserMessage($"Service Name: {serviceName}"));

            try
            {
                var response = await openAIClient.GetChatCompletionsAsync(thread, stoppingToken);
                var rawContent = response.Value.Choices[0].Message.Content;
                var mapResponse = AreaPathMapResponse.FromJson(rawContent, jsonSerializerOptions);

                var validAreaPaths = areaPathServiceMapConfig.Map.Select(m => m.RouteToAreaPath).ToList();
                validAreaPaths.Add(areaPathServiceMapConfig.DefaultAreaPath);

                if (mapResponse != null)
                {
                    if (!validAreaPaths.Contains($"{mapResponse.AreaPath}"))
                    {
                        return new AreaPathMapResponse { AreaPath = string.Empty };
                    }
                    else 
                    {
                        return mapResponse;
                    }                    
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, message: "");
            }
            return null;
        }
    }
}

