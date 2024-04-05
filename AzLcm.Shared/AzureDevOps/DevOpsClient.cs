


using AzLcm.Shared.AzureDevOps.Abstracts;
using AzLcm.Shared.AzureDevOps.Authorizations;
using AzLcm.Shared.Cognition.Models;
using AzLcm.Shared.Policy.Models;
using AzLcm.Shared.Storage;
using Microsoft.Extensions.Logging;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzLcm.Shared.AzureDevOps
{
    public class DevOpsClient(        
        JsonSerializerOptions jsonSerializerOptions,
        AzureDevOpsClientConfig appConfiguration,
        IHttpClientFactory httpClientFactory,
        AuthorizationFactory identitySupport,
        ILogger<DevOpsClient> logger) : ClientBase(jsonSerializerOptions,
            appConfiguration, identitySupport, httpClientFactory)
    {
        public async Task<ConnectionDataPayload> GetConnectionDataAsync(string orgName)
        {
            var apiPath = $"_apis/ConnectionData";
            var connectionData = await this.GetAsync<ConnectionDataPayload>(orgName, apiPath, false);
            return connectionData;
        }

        public async Task CreateWorkItemFromFeedAsync(
            string orgName, 
            WorkItemTemplate? template, 
            SyndicationItem feed,
            Verdict? verdict)
        {
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentNullException.ThrowIfNull(feed, nameof(feed));            
            ArgumentException.ThrowIfNullOrWhiteSpace(orgName, nameof(orgName));

            var apiPath = $"{template.ProjectId}/_apis/wit/workitems/${template.Type}?api-version=7.1-preview.3";
            var workItemTags = string.Empty;
            if (verdict != null)
            {
                var tags = new List<string>();
                if(verdict.Actionable) tags.Add("Actionable");
                if(verdict.AnnouncementRequired) tags.Add("Announcement");
                if(verdict.ActionableViaAzurePolicy) tags.Add("AzurePolicy");
                tags.Add(verdict.UpdateKind.ToString());
                workItemTags = string.Join(", ", tags);
            }

            if (template.Fields != null)
            {
                var fields = new List<PatchFragment>();
                foreach (var tplField in template.Fields)
                {
                    var tplValue = $"{tplField.Value}";

                    tplValue = tplValue.Replace("{Feed.Title}", feed.Title.Text);
                    tplValue = tplValue.Replace("{Feed.Id}", feed.Id);
                    tplValue = tplValue.Replace("{Feed.Summary}", feed.Summary.Text);
                    tplValue = tplValue.Replace("{Feed.PublishDate}", feed.PublishDate.ToString());

                    if(feed.Links != null && feed.Links.Any())
                    {
                        var link = feed.Links.First();
                        tplValue = tplValue.Replace("{Feed.Link}", link.Uri.ToString());
                    }
                    var textContent = feed.Content as System.ServiceModel.Syndication.TextSyndicationContent;
                    if (textContent != null)
                    {
                        tplValue = tplValue.Replace("{Feed.Content.Text}", textContent.Text);
                    }

                    if(verdict != null)
                    {
                        tplValue = tplValue.Replace("{Classification.Kind}", verdict.UpdateKind.ToString());
                        if(verdict.AzureServiceNames != null)
                        {
                            tplValue = tplValue.Replace("{Classification.ServiceName}", string.Join(", ", verdict.AzureServiceNames));
                        }                        
                        tplValue = tplValue.Replace("{Classification.AiSuggestion}", $"{verdict.MitigationInstructionMarkdown}");
                        tplValue = tplValue.Replace("{Classification.Tags}", workItemTags);
                    }

                    fields.Add(new PatchFragment
                    {
                        Op = tplField.Op,
                        Path = tplField.Path,
                        Value = tplValue
                    });
                }
                await this.PostAsync<object, string>(orgName, apiPath, fields, false, "application/json-patch+json");
            }
        }

        public async Task CreateWorkItemFromPolicyAsync(
            string orgName,
            WorkItemTemplate? template,
            PolicyModelChange policyUpdate)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(orgName, nameof(orgName));
            ArgumentNullException.ThrowIfNull(template, nameof(template));
            ArgumentNullException.ThrowIfNull(policyUpdate, nameof(policyUpdate));
            ArgumentNullException.ThrowIfNull(policyUpdate.Policy, nameof(policyUpdate.Policy));            

            var apiPath = $"{template.ProjectId}/_apis/wit/workitems/${template.Type}?api-version=7.1-preview.3";
            
            var tags = new List<string>();
            var workItemTags = string.Empty;
            if (policyUpdate.Changes != null)
            {
                var versionChange = policyUpdate.Changes.VersionChange;
                if (versionChange != null)
                {
                    tags.Add(versionChange.VersionChangeKind.ToString());
                    tags.Add(versionChange.NewVersion);

                    if(policyUpdate.Changes.WasGA)
                    {
                        tags.Add("Public");
                    }
                    if (policyUpdate.Changes.HasDeprecated)
                    {
                        tags.Add("Deprecated");
                    }
                }

                if(policyUpdate.Policy != null && template.Fields != null)
                {
                    var policy = policyUpdate.Policy;
                    var fields = new List<PatchFragment>();
                    foreach (var tplField in template.Fields)
                    {
                        var tplValue = $"{tplField.Value}";

                        tplValue = tplValue.Replace("{Policy.Id}", policy.Id);
                        tplValue = tplValue.Replace("{Policy.Name}", policy.Name);
                        tplValue = tplValue.Replace("{Policy.DisplayName}", policy.Properties.DisplayName);
                        tplValue = tplValue.Replace("{Policy.Description}", policy.Properties.Description);
                        tplValue = tplValue.Replace("{Policy.Version}", policy.Properties.Metadata.Version);
                        tplValue = tplValue.Replace("{Policy.Mode}", policy.Properties.Mode);
                        tplValue = tplValue.Replace("{Policy.PolicyType}", policy.Properties.PolicyType);
                        tplValue = tplValue.Replace("{Policy.Category}", policy.Properties.Metadata.Category);
                        tplValue = tplValue.Replace("{Policy.Preview}", policy.Properties.Metadata.Preview.ToString());
                        tplValue = tplValue.Replace("{Policy.Deprecated}", policy.Properties.Metadata.Deprecated.ToString());
                        tplValue = tplValue.Replace("{Classification.Tags}", string.Join(", ", tags));

                        if (versionChange != null)
                        {
                            tplValue = tplValue.Replace("{Policy.VersionChange}", versionChange.VersionChangeKind.ToString());
                            tplValue = tplValue.Replace("{Policy.LatestVersion}", versionChange.NewVersion);
                        }

                        fields.Add(new PatchFragment
                        {
                            Op = tplField.Op,
                            Path = tplField.Path,
                            Value = tplValue
                        });
                    }
                    await this.PostAsync<object, string>(orgName, apiPath, fields, false, "application/json-patch+json");
                }
            }
        }
    }

    public record ConnectionDataPayload(
        [property: JsonPropertyName("authenticatedUser")] AzureDevOpsUserContext AuthenticatedUser,
        [property: JsonPropertyName("authorizedUser")] AzureDevOpsUserContext AuthorizedUser,
        [property: JsonPropertyName("instanceId")] string InstanceId,
        [property: JsonPropertyName("deploymentId")] string DeploymentId,
        [property: JsonPropertyName("deploymentType")] string DeploymentType,
        [property: JsonPropertyName("locationServiceData")] LocationServiceData LocationServiceData
    );
    public record LocationServiceData(
        [property: JsonPropertyName("serviceOwner")] string ServiceOwner,
        [property: JsonPropertyName("defaultAccessMappingMoniker")] string DefaultAccessMappingMoniker,
        [property: JsonPropertyName("lastChangeId")] long LastChangeId,
        [property: JsonPropertyName("lastChangeId64")] long LastChangeId64
    );

    public record AzureDevOpsUserContext(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("descriptor")] string Descriptor,
        [property: JsonPropertyName("subjectDescriptor")] string SubjectDescriptor,
        [property: JsonPropertyName("providerDisplayName")] string ProviderDisplayName,
        [property: JsonPropertyName("isActive")] bool IsActive
    );
}
