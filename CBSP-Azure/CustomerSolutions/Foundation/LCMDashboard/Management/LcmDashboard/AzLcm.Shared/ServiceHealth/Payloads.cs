

using System.Text.Json.Serialization;

namespace AzLcm.Shared.ServiceHealth
{
    public record ServiceHealthEvent(
        [property: JsonPropertyName("lastUpdate")] DateTime? LastUpdate,
        [property: JsonPropertyName("eventType")] string EventType,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("service")] string Service,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("summary")] string Summary,
        [property: JsonPropertyName("url")] string Url
    );

    public record ServiceHealthCollection(
        [property: JsonPropertyName("totalRecords")] int? TotalRecords,
        [property: JsonPropertyName("count")] int? Count,
        [property: JsonPropertyName("data")] IReadOnlyList<ServiceHealthEvent> Events,
        //[property: JsonPropertyName("facets")] IReadOnlyList<object> Facets,
        [property: JsonPropertyName("resultTruncated")] string ResultTruncated
    );

}
