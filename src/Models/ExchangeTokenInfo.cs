namespace StravaUtilities;

/// <summary>
/// A class with info about the intial athlete auth exchange, what Strava gives when redirecting after the user authorizes
/// </summary>
public class ExchangeTokenInfo
{
    /// <summary>
    /// The code that can be used to exchange for access token info
    /// </summary>
    public string AuthorizationCode { get; set; } = "";

    /// <summary>
    /// The scopes that the athlete authorized
    /// </summary>
    public List<Scope> Scopes { get; set; } = [];
}
