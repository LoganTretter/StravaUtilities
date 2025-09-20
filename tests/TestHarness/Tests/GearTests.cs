using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class GearTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal async Task GetShoes()
    {
        var shoes = await stravaApiClient.GetShoes(_options.AthleteId).ConfigureAwait(false);

        ;
    }

    internal async Task GetBikes()
    {
        var bikes = await stravaApiClient.GetBikes(_options.AthleteId).ConfigureAwait(false);

        ;
    }
}
