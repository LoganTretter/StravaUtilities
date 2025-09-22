namespace StravaUtilities;

/// <summary>
/// Auth info for an athlete to the Strava API application
/// </summary>
public class StravaApiAthleteAuthInfo
{
    /// <summary>
    /// The id of the athlete this info is for
    /// </summary>
    public long AthleteId { get; set; }

    /// <summary>
    /// The token info
    /// </summary>
    public StravaApiTokenInfo? TokenInfo { get; set; }

    /// <summary>
    /// The scopes the athlete has authorized for the API application
    /// </summary>
    public List<Scope> Scopes { get; set; } = [];
}

/// <summary>
/// Token info to the Strava API application
/// </summary>
public class StravaApiTokenInfo : IEquatable<StravaApiTokenInfo>
{
    /// <summary>
    /// The access token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// The refresh token that can be used to get a new access token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// When the access token expires
    /// </summary>
    public DateTimeOffset? AccessTokenExpiration { get; set; }

    public bool Equals(StravaApiTokenInfo? other)
    {
        return other != null
            && AccessToken == other.AccessToken
            && RefreshToken == other.RefreshToken
            && Nullable.Equals(AccessTokenExpiration, other.AccessTokenExpiration);
    }
}
