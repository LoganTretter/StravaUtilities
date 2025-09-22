using System.Text.Json.Serialization;

namespace StravaUtilities;

/// <summary>
/// The response given by Strava on a token request
/// </summary>
internal class TokenResponse
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

/// <summary>
/// The response given on the very first token request, when exchanging the initial auth code for a token
/// </summary>
internal class InitialAuthResponse : TokenResponse
{
    [JsonPropertyName("athlete")]
    public MetaAthlete? Athlete { get; set; } // they only provide this on the initial auth call, not refreshes
}