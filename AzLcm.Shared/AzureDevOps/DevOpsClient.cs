


using AzLcm.Shared.AzureDevOps.Abstracts;
using AzLcm.Shared.AzureDevOps.Authorizations;
using Microsoft.Extensions.Logging;
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
