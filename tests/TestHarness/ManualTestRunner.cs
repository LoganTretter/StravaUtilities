using StravaUtilities.TestHarness.Tests;

namespace StravaUtilities.TestHarness;

internal class ManualTestRunner(
    ActivityTests activityTests,
    ActivityUploadTests activityUploadTests,
    AthleteTests athleteTests,
    GearTests gearTests,
    SecretClientExtensionsTests secretClientExtensionsTests)
{
    internal async Task Run()
    {
        // This is just an entry point that can be used to run whatever test I want
        // in lieu of actual unit tests, since I want to actually call the Strava API, not just test against mocks

        await activityTests.GetActivitiesInPages();
    }
}
