

using AzLcm.Shared.AzureDevOps.Authorizations;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AzLcm.Shared.AzureDevOps.Abstracts
{
    public abstract class ClientBase
    {
        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly AzureDevOpsClientConfig appConfiguration;
        private readonly AuthorizationFactory identitySupport;
        private readonly ILogger<DevOpsClient> logger;
        private readonly IHttpClientFactory httpClientFactory;

        protected ClientBase(
            JsonSerializerOptions jsOptions,
            AzureDevOpsClientConfig appConfig,
            AuthorizationFactory idnSupport,
            ILogger<DevOpsClient> logger,
            IHttpClientFactory httpClientFactory)
        {
            this.jsonSerializerOptions = jsOptions;
            this.appConfiguration = appConfig;
            this.identitySupport = idnSupport;
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        protected async virtual Task<TResponsePayload> PostAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, CancellationToken cancellationToken, 
            bool elevate = false, string contentType = "")
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
                orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, 
                HttpMethod.Post,cancellationToken , elevate, contentType);
        }

        protected async virtual Task<TResponsePayload> PutAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, 
            CancellationToken cancellationToken, bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
                orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, 
                HttpMethod.Put, cancellationToken, elevate);
        }

        protected async virtual Task<TResponsePayload> PatchAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, CancellationToken cancellationToken,
            bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
                orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, payload, HttpMethod.Patch, 
                cancellationToken, elevate);
        }

        protected async virtual Task<bool> PatchWithoutBodyAsync(
            string orgName, string apiPath, CancellationToken cancellationToken, bool elevate = false)
        {
            return await SendRequestWithoutBodyCoreAsync(
                orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, HttpMethod.Patch, cancellationToken, elevate);
        }

        protected async virtual Task<TPayload> GetAsync<TPayload>(
            string orgName, string apiPath, CancellationToken cancellationToken, 
            bool elevate = false) where TPayload : class
        {
            return await GetCoreAsync<TPayload>(
                orgName, AzureDevOpsClientConstants.CoreAPI.NAME, apiPath, cancellationToken, elevate);
        }

        protected async virtual Task<TPayload> GetVsspAsync<TPayload>(
            string orgName, string apiPath, CancellationToken cancellationToken, 
            bool elevate = false) where TPayload : class
        {
            return await GetCoreAsync<TPayload>(
                orgName, AzureDevOpsClientConstants.VSSPS_API.NAME, apiPath, cancellationToken, elevate);
        }

        protected async virtual Task<TResponsePayload> PostVsspAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiPath, TRequestPayload payload, CancellationToken cancellationToken, 
            bool elevate = false)
            where TRequestPayload : class
            where TResponsePayload : class
        {
            return await SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
                orgName, AzureDevOpsClientConstants.VSSPS_API.NAME, apiPath, payload, 
                HttpMethod.Post, cancellationToken, elevate);
        }

        private async Task<TPayload> GetCoreAsync<TPayload>(
            string orgName, string apiType, string apiPath, CancellationToken cancellationToken,
            bool elevate = false) where TPayload : class
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(cancellationToken, elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<TPayload>(this.jsonSerializerOptions, cancellationToken);
                if (result != null)
                {
                    return result;
                }
            }
            throw new InvalidOperationException($"Error: {response.StatusCode}");
        }

        private async Task<TResponsePayload> SendRequestCoreAsync<TRequestPayload, TResponsePayload>(
            string orgName, string apiType, string apiPath,
            TRequestPayload payload,
            HttpMethod httpMethod, CancellationToken cancellationToken,
            bool elevate = false,
            string contentType = "application/json")
            where TRequestPayload : class
            where TResponsePayload : class
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(cancellationToken, elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync<TRequestPayload>(
                memoryStream, payload, this.jsonSerializerOptions, cancellationToken);

            var jsonContent = new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), 
                Encoding.UTF8, contentType);
            var request = new HttpRequestMessage(httpMethod, path)
            {
                Content = jsonContent
            };
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var x = await response.Content.ReadAsStringAsync(cancellationToken);
                if (typeof(TResponsePayload) == typeof(string))
                {
#pragma warning disable CS8603 // Possible null reference return.
                    return (!string.IsNullOrWhiteSpace(x as string) ? x as string : string.Empty) as TResponsePayload;
#pragma warning restore CS8603 // Possible null reference return.
                }
                var result = await response.Content.ReadFromJsonAsync<TResponsePayload>(this.jsonSerializerOptions, cancellationToken);
                if (result != null)
                {
                    return result;
                }
            }
            else
            {
                LogError(message: "Error Occurred in SendRequestCoreAsync(). {httpMethod} {orgName} {apiPath} ", httpMethod, orgName, apiPath);

                try
                {
                    var x = await response.Content.ReadAsStringAsync();
                    LogError(message: "Error Occurred in SendRequestCoreAsync(). {response} ", x);
                }
                catch { }
            }
            throw new InvalidOperationException($"Error: {response.StatusCode}");
        }

        private async Task<bool> SendRequestWithoutBodyCoreAsync(
            string orgName, string apiType, string apiPath, 
            HttpMethod httpMethod, CancellationToken cancellationToken, bool elevate = false)
        {
            var (scheme, token) = await identitySupport.GetCredentialsAsync(cancellationToken, elevate);
            using HttpClient client = httpClientFactory.CreateClient(apiType);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, token);
            var path = $"/{orgName}/{apiPath}";

            var request = new HttpRequestMessage(httpMethod, path);
            var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        protected void LogError(string? message, params object?[] args)
        {
            logger?.LogError(message, args);
        }
    }
}
