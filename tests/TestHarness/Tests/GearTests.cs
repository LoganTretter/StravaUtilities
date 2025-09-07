namespace StravaUtilities.TestHarness.Tests;

internal class GearTests(StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    internal async Task GetShoes()
    {
        var shoes = await stravaApiClient.GetShoes().ConfigureAwait(false);

        ;
    }

    internal async Task GetBikes()
    {
        var bikes = await stravaApiClient.GetBikes().ConfigureAwait(false);

        ;
    }
}
