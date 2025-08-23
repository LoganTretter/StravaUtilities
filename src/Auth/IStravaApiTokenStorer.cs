namespace StravaUtilities;

/// <summary>
/// Defines methods for storing strava token information somewhere
/// </summary>
public interface IStravaApiTokenStorer
{
    /// <summary>
    /// Get Strava API token info from storage
    /// </summary>
    /// <returns></returns>
    Task<StravaApiToken> GetToken();

    /// <summary>
    /// Add or update Strava API token info into storage
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task AddOrUpdateToken(StravaApiToken token);
}
