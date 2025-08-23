namespace StravaUtilities;

public partial class StravaApiClient
{
    public async Task<List<Shoe>> GetShoes()
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var shoes = CurrentAuthenticatedAthlete.Shoes;
        return await GetDetailedGears<Shoe>(shoes.Select(s => s.Id)).ConfigureAwait(false);
    }

    public async Task<List<Bike>> GetBikes()
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var bikes = CurrentAuthenticatedAthlete.Bikes;
        return await GetDetailedGears<Bike>(bikes.Select(b => b.Id)).ConfigureAwait(false);
    }

    private async Task<List<T>> GetDetailedGears<T>(IEnumerable<string> gearIds) where T : Gear
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        var gears = new List<T>();

        // TODO this wasn't working in parallel

        //var tasks = new List<Task>();
        foreach (var gearId in gearIds)
        {
            //tasks.Add(new Task(async () =>
            //{
                var gear = await HttpClient.Get<T>($"gear/{gearId}").ConfigureAwait(false);
                gears.Add(gear);
            //}));
        }
        //await Task.WhenAll(tasks).ConfigureAwait(false);

        return gears.OrderBy(g => g.Name).ToList();
    }
}