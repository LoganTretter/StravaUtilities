using System.Text.Json.Serialization;

namespace StravaUtilities;

public class Gear
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("resource_state")]
    public ResourceState ResourceState { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("nickname")]
    public string NickName { get; set; }

    [JsonPropertyName("brand_name")]
    public string Brand { get; set; }

    [JsonPropertyName("model_name")]
    public string Model { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("distance")]
    public decimal DistanceMeters { get; set; }

    [JsonPropertyName("retired")]
    public bool Retired { get; set; }

    [JsonPropertyName("primary")]
    public bool IsPrimary { get; set; }
}