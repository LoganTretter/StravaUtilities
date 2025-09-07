namespace StravaUtilities.TestHarness.Tests;

internal class AthleteTests(StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    internal async Task Run()
    {
        await GetAthlete().ConfigureAwait(false);
    }

    private async Task GetAthlete()
    {
        var athlete = await stravaApiClient.GetAthlete().ConfigureAwait(false);

        ;
    }
}
