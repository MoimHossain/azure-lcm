

using AzLcm.Shared.Policy.Models;
using AzLcm.Shared.Storage;
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
        public async Task ReadPoliciesAsync(Func<PolicyModel, Task> work, PolicyStorage policyStorage, CancellationToken stoppingToken)
        {
            var lastProcessedGitHubCommitSha = await policyStorage.GetLastProcessedShaAsync(stoppingToken);


            var recentCommits = await GetRecentCommitsAsync(maxCommitsToRead: 50, stoppingToken);

            if (recentCommits != null && recentCommits.Count > 0)
            {   
                List<GitHubCommitSlim> unseenCommits = [];
                foreach(var recentCommit in recentCommits)
                {
                    if (!string.IsNullOrWhiteSpace(lastProcessedGitHubCommitSha)
                        && lastProcessedGitHubCommitSha.Equals(recentCommit.Sha, StringComparison.Ordinal))
                    {
                        break; // we will skip the rest of the commits
                    }
                    else 
                    {
                        unseenCommits.Add(recentCommit);
                    }
                }

                var changedFiles = new Dictionary<string, GitHubCommittedFile>();
                foreach (var commit in unseenCommits.Reverse<GitHubCommitSlim>())
                {
                    var commitDetails = await GetCommitDetailsAsync(commit.Sha, stoppingToken);
                    if (commitDetails != null && commitDetails.Files != null)
                    {
                        foreach (var file in commitDetails.Files)
                        {
                            if (file.Filename.Contains("Azure Government", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            if (file.Filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                            {
                                changedFiles[file.Filename] = file;
                            }
                        }
                    }
                }

                foreach(var changedFile in changedFiles.Values)
                {
                    try
                    {
                        var policy = await httpClientFactory.CreateClient().GetFromJsonAsync<PolicyModel>(changedFile.RawUrl, stoppingToken);
                        if (work != null && policy != null)
                        {
                            await work(policy);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to read file {fileName}", changedFile.Filename);
                    }
                }

                var mostRecentCommit = recentCommits[0];
                if(mostRecentCommit != null )
                {
                    await policyStorage.UpdateLastProcessedShaAsync(mostRecentCommit.Sha, stoppingToken);
                }                
            }
        }

        private async Task<GitHubCommitDetails?> GetCommitDetailsAsync(string sha, CancellationToken stoppingToken)
        {
            GitHubCommitDetails? ghCommitDetails = null;
            try
            {
                var request = await CreateRequestObject(HttpMethod.Get, $"{GitHubAzurePolicyRepoURI}/commits/{sha}", stoppingToken);
                var gitHubResponse = await httpClientFactory.CreateClient().SendAsync(request, stoppingToken);
                if (gitHubResponse.IsSuccessStatusCode)
                {
                    ghCommitDetails = await gitHubResponse.Content.ReadFromJsonAsync<GitHubCommitDetails>(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get commit details");
            }
            return ghCommitDetails;
        }

        private async Task<List<GitHubCommitSlim>> GetRecentCommitsAsync(int maxCommitsToRead, CancellationToken stoppingToken)
        {
            List<GitHubCommitSlim> ghCommits = [];
            try 
            {
                var request = await CreateRequestObject(HttpMethod.Get, $"{GitHubAzurePolicyRepoURI}/commits?per_page={maxCommitsToRead}", stoppingToken);
                var gitHubResponse = await httpClientFactory.CreateClient().SendAsync(request, stoppingToken);
                if(gitHubResponse.IsSuccessStatusCode)
                {
                    var items  = await gitHubResponse.Content.ReadFromJsonAsync<List<GitHubCommitSlim>>(stoppingToken);                    
                    if(items != null )
                    {
                        ghCommits = items;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get recent commits");
            }
            return ghCommits;
        }


        private async Task<HttpRequestMessage> CreateRequestObject(HttpMethod method, string uri, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.UserAgent.ParseAdd("policy-daemon");
            if (!string.IsNullOrWhiteSpace(daemonConfig.GitHubPAT))
            {
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {daemonConfig.GitHubPAT}");
            }
            else
            {
                await Task.Delay(2000, cancellationToken); // Avoid throttling by GitHub
            }
            return request;
        }


        private string GitHubAzurePolicyRepoURI => "https://api.github.com/repos/azure/azure-policy";
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

    public record GitHubCommitSlim(
        [property: JsonPropertyName("sha")] string Sha,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("comments_url")] string CommentsUrl
    );

    
    public record GitHubCommittedFile(
        [property: JsonPropertyName("sha")] string Sha,
        [property: JsonPropertyName("filename")] string Filename,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("additions")] int Additions,
        [property: JsonPropertyName("deletions")] int Deletions,
        [property: JsonPropertyName("changes")] int Changes,
        [property: JsonPropertyName("blob_url")] string BlobUrl,
        [property: JsonPropertyName("raw_url")] string RawUrl,
        [property: JsonPropertyName("contents_url")] string ContentsUrl,
        [property: JsonPropertyName("patch")] string Patch
    );

    public record GitHubCommitDetails(
        [property: JsonPropertyName("sha")] string Sha,
        [property: JsonPropertyName("node_id")] string NodeId,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("comments_url")] string CommentsUrl,
        [property: JsonPropertyName("stats")] GitHubCommittedFileStats Stats,
        [property: JsonPropertyName("files")] IReadOnlyList<GitHubCommittedFile> Files
    );

    public record GitHubCommittedFileStats(
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("additions")] int Additions,
        [property: JsonPropertyName("deletions")] int Deletions
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