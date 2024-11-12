

using AzLcm.Shared.Policy.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.Policy
{
    public class PolicyReader(
        DaemonConfig daemonConfig,
        ILogger<PolicyReader> logger,
        IHttpClientFactory httpClientFactory)
    {
        public async Task ReadPoliciesAsync(Func<PolicyModel, Task> work, CancellationToken stoppingToken)
        {
            await ExploreDirectoryAsync($"{GetBaseURI()}{GetPath()}", work, stoppingToken);
        }

        private async Task ExploreDirectoryAsync(string uri, Func<PolicyModel, Task> work, CancellationToken stoppingToken)
        {
            var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.SendAsync(await CreateRequestBody(uri), stoppingToken);

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<GitHubItem>>(stoppingToken);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if(item.Path.Contains("Azure Government", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (item.IsFile())
                        {
                            await ProcessPolicyFileAsync(work, item, stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(1000, stoppingToken); // Avoid throttling by GitHub
                            await ExploreDirectoryAsync($"{GetBaseURI()}{item.Path}", work, stoppingToken);
                        }
                    }
                }
            }
            else
            {
                var failedResponse = await response.Content.ReadAsStringAsync(stoppingToken);
                logger.LogError("URI {uri} failed with reason {failedResponse}", uri, failedResponse);
            }
        }

        private async Task ProcessPolicyFileAsync(Func<PolicyModel, Task> work, GitHubItem item, CancellationToken stoppingToken)
        {
            if (!string.IsNullOrWhiteSpace(item.DownloadUrl))
            {
                try
                {
                    var httpClient = httpClientFactory.CreateClient();
                    var policy = await httpClient.GetFromJsonAsync<PolicyModel>(item.DownloadUrl, stoppingToken);
                    if (work != null && policy != null)
                    {
                        await work(policy);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to read file {fileName}", item.Path);
                }
            }
        }

        private async Task<HttpRequestMessage> CreateRequestBody(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.UserAgent.ParseAdd("policy-daemon");
            if (!string.IsNullOrWhiteSpace(daemonConfig.GitHubPAT))
            {
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {daemonConfig.GitHubPAT}");
            }
            else
            {
                await Task.Delay(2000); // Avoid throttling by GitHub
            }
            return request;
        }

        private string GetBaseURI()
        {
            var uri = "https://api.github.com/repos/azure/azure-policy/contents/";
            if (!string.IsNullOrWhiteSpace(daemonConfig.AzurePolicyGitHubBaseURI))
            {
                uri = daemonConfig.AzurePolicyGitHubBaseURI;
            }
            return uri;
        }

        private string GetPath()
        {
            var path = "built-in-policies/policyDefinitions";
            if (!string.IsNullOrWhiteSpace(daemonConfig.AzurePolicyPath))
            {
                path = daemonConfig.AzurePolicyPath;
            }
            return path;
        }
    }

    public record GitHubItem(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("path")] string Path,
        [property: JsonPropertyName("sha")] string Sha,
        [property: JsonPropertyName("size")] int size,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("git_url")] string GitUrl,
        [property: JsonPropertyName("download_url")] string DownloadUrl,
        [property: JsonPropertyName("type")] string Type
    );

    public static class GitHubContentTypeExtensions
    {
        public static bool IsFile(this GitHubItem gitHubItem)
        {
            return gitHubItem != null &&
                !string.IsNullOrWhiteSpace(gitHubItem.Type) &&
                "file".Equals(gitHubItem.Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}
