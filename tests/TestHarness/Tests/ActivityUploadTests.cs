using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class ActivityUploadTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal async Task UploadActivityFromTcxFile()
    {
        // Either create a file manually or download one from Strava or other app, so there is something to upload
        // This will upload it to Strava
        // Then should go delete it on Strava after since API can't do that

        var uploadInfo = new ActivityUploadInfo
        {
            SourceFilePath = _options.ActivityToUploadFilePath,
            SourceDataFormat = DataFormat.Tcx,
            ActivityName = $"Upload Test at {DateTime.UtcNow.ToString("O")}",
            ExternalId = $"upload-test-at-{DateTime.UtcNow.ToString("O")}",
            ActivityType = ActivityType.Run,
            Description = "testing upload from StravaUtilities test harness",
            Private = true,
            Trainer = false,
            Commute = true,
            WorkoutType = WorkoutType.RunWorkout,
            Effort = 7,
            DeviceName = "test device",
            SuppressFromFeed = true
        };

        await stravaApiClient.UploadActivityAndWaitForCompletion(uploadInfo, _options.AthleteId).ConfigureAwait(false);
    }
}
