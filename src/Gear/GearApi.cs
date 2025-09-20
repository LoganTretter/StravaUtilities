namespace StravaUtilities;

public partial class StravaApiClient
{
    public async Task<List<Shoe>> GetShoes(long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        var athlete = await GetAthlete(athleteId, authInfo).ConfigureAwait(false);

        var shoes = athlete.Shoes;
        return await GetGearsWithAllDetails<Shoe>(shoes.Select(s => s.Id), athleteId, authInfo).ConfigureAwait(false);
    }

    public async Task<List<Bike>> GetBikes(long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        var athlete = await GetAthlete(athleteId, authInfo).ConfigureAwait(false);

        var bikes = athlete.Bikes;
        return await GetGearsWithAllDetails<Bike>(bikes.Select(b => b.Id), athleteId).ConfigureAwait(false);
    }

    public async Task<List<T>> GetGearsWithAllDetails<T>(IEnumerable<string> gearIds, long athleteId, StravaApiAthleteAuthInfo? authInfo = null) where T : Gear
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        var athlete = await GetAthlete(athleteId, authInfo).ConfigureAwait(false);

        var gearsTemp = await Task.WhenAll(gearIds.Select(gearId => GetGearWithAllDetails<T>(gearId, authInfo))).ConfigureAwait(false);

        return gearsTemp.OrderBy(g => g.Name).ToList();
    }

    public async Task<T> GetGearWithAllDetails<T>(string gearId, long athleteId, StravaApiAthleteAuthInfo? authInfo = null) where T : Gear
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        return await GetGearWithAllDetails<T>(gearId, authInfo).ConfigureAwait(false);
    }

    public async Task<T> GetGearWithAllDetails<T>(string gearId, StravaApiAthleteAuthInfo authInfo) where T : Gear
    {
        return await StravaHttpClient.Get<T>($"{ApiPath}/gear/{gearId}", authInfo).ConfigureAwait(false);
    }
}