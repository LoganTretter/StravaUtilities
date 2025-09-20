using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class AthleteTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal async Task GetAthlete()
    {
        var athlete = await stravaApiClient.GetAthlete(_options.AthleteId).ConfigureAwait(false);

        ;
    }
}
