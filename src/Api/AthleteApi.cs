namespace StravaUtilities;

public partial class StravaApiClient
{
    public async Task<Athlete> GetAthlete(long id, StravaApiAthleteAuthInfo? authInfo = null)
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(id).ConfigureAwait(false);

        var athlete = await StravaHttpClient.Get<Athlete>($"{ApiPath}/athlete", authInfo).ConfigureAwait(false);

        return athlete;
    }
}