namespace StravaUtilities;

/// <summary>
/// Defines methods for storing strava auth information somewhere
/// </summary>
public interface IStravaApiAthleteAuthInfoStorer
{
    /// <summary>
    /// Get auth info for the Strava API for a certain athlete from storage
    /// </summary>
    /// <param name="athleteId">The id of the athlete the auth info is for</param>
    /// <returns>The token</returns>
    Task<StravaApiAthleteAuthInfo> GetAthleteAuthInfo(long athleteId);

    /// <summary>
    /// Add or update athlete auth info for the Strava API into storage
    /// </summary>
    /// <param name="authInfo">The athlete auth info to store</param>
    /// <returns></returns>
    Task AddOrUpdateAthleteAuthInfo(StravaApiAthleteAuthInfo authInfo);
}
