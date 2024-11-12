

using System.Text.Json.Serialization;

namespace AzLcm.Shared.Policy.Models
{
    public record PolicyMetadata(
        [property: JsonPropertyName("version")] string Version,
        [property: JsonPropertyName("category")] string Category,
        [property: JsonPropertyName("deprecated")] bool Deprecated,
        [property: JsonPropertyName("preview")] bool Preview,        
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("description")] string Description
    );

    public record PolicyProperties(
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("policyType")] string PolicyType,
        [property: JsonPropertyName("mode")] string Mode,
        [property: JsonPropertyName("metadata")] PolicyMetadata Metadata,
        [property: JsonPropertyName("version")] string Version,               
        [property: JsonPropertyName("versions")] IReadOnlyList<string> Versions
    );

    public record PolicyModel(
        [property: JsonPropertyName("properties")] PolicyProperties Properties,
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name
    );

    public enum ChangeKind
    {
        None = 0,
        Add = 1,
        Update = 2
    }

    public record PolicyModelChange(
        [property: JsonPropertyName("changeKind")] ChangeKind ChangeKind,
        [property: JsonPropertyName("policy")] PolicyModel Policy,
        [property: JsonPropertyName("changes")] PolicyChangeCollection? Changes
    );
}
