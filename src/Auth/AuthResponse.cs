using System.Text.Json.Serialization;

namespace StravaUtilities;

/// <summary>
/// The response given by Strava on a token request
/// </summary>
public class StravaAuthResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_at")]
    public long ExpiresAtSecondsSinceEpoch { get; set; }
    [JsonPropertyName("expires_in")]
    public long ExpiresInSeconds { get; set; }
}