namespace StravaUtilities;

public partial class StravaApiClient
{
    public async Task<List<Shoe>> GetShoes()
    {
        var athlete = await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var shoes = athlete.Shoes;
        return await GetGearsWithAllDetails<Shoe>(shoes.Select(s => s.Id)).ConfigureAwait(false);
    }

    public async Task<List<Bike>> GetBikes()
    {
        var athlete = await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var bikes = athlete.Bikes;
        return await GetGearsWithAllDetails<Bike>(bikes.Select(b => b.Id)).ConfigureAwait(false);
    }

    public async Task<List<T>> GetGearsWithAllDetails<T>(IEnumerable<string> gearIds) where T : Gear
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var gearsTemp = await Task.WhenAll(gearIds.Select(GetGearWithAllDetails<T>)).ConfigureAwait(false);

        return gearsTemp.OrderBy(g => g.Name).ToList();
    }

    public async Task<T> GetGearWithAllDetails<T>(string gearId) where T : Gear
    {
        return await HttpClient.Get<T>($"gear/{gearId}").ConfigureAwait(false);
    }
}