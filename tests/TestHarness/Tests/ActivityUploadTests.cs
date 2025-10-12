using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class ActivityUploadTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    // Either create a file manually or download one from Strava or other app, so there is something to upload
    // This will upload it to Strava
    // Then should go delete it on Strava after since API can't do that

    internal async Task UploadActivityFromTcxFile()
    {
        var uploadInfo = new ActivityUploadAndUpdateFromFile
        {
            ActivityFilePath = _options.ActivityToUploadFilePath,
            SourceDataFormat = DataFormat.Tcx,
            ActivityName = $"Upload Test at {DateTime.UtcNow.ToString("O")}",
            ExternalId = $"upload-test-at-{DateTime.UtcNow.ToString("O")}",
            ActivityType = ActivityType.Run,
            Description = "testing upload from StravaUtilities test harness",
            Private = true,
            Trainer = true,
            Commute = true,
            WorkoutType = WorkoutType.RunWorkout,
            Effort = 7,
            GearId = _options.GearId,
            SuppressFromFeed = true
        };

        await stravaApiClient.UploadActivityAndWaitForCompletion(uploadInfo, _options.AthleteId).ConfigureAwait(false);
    }

    internal async Task UploadActivityFromBytes()
    {
        var fileBytes = await File.ReadAllBytesAsync(_options.ActivityToUploadFilePath).ConfigureAwait(false);

        var uploadInfo = new ActivityUploadAndUpdateFromBytes
        {
            ActivityFileBytes = fileBytes,
            SourceDataFormat = DataFormat.Tcx,
            ActivityName = $"Upload Test at {DateTime.UtcNow.ToString("O")}",
            ExternalId = $"upload-test-at-{DateTime.UtcNow.ToString("O")}",
            ActivityType = ActivityType.Run,
            Description = "testing upload from StravaUtilities test harness",
            Private = true,
            Trainer = true,
            Commute = true,
            WorkoutType = WorkoutType.RunWorkout,
            Effort = 7,
            GearId = _options.GearId,
            SuppressFromFeed = true
        };

        await stravaApiClient.UploadActivityAndWaitForCompletion(uploadInfo, _options.AthleteId).ConfigureAwait(false);
    }

    internal async Task UploadActivityFromString()
    {
        var fileString = await File.ReadAllTextAsync(_options.ActivityToUploadFilePath).ConfigureAwait(false);

        var uploadInfo = new ActivityUploadAndUpdateFromString
        {
            ActivityFileString = fileString,
            SourceDataFormat = DataFormat.Tcx,
            ActivityName = $"Upload Test at {DateTime.UtcNow.ToString("O")}",
            ExternalId = $"upload-test-at-{DateTime.UtcNow.ToString("O")}",
            ActivityType = ActivityType.Run,
            Description = "testing upload from StravaUtilities test harness",
            Private = true,
            Trainer = true,
            Commute = true,
            WorkoutType = WorkoutType.RunWorkout,
            Effort = 7,
            GearId = _options.GearId,
            SuppressFromFeed = true
        };

        await stravaApiClient.UploadActivityAndWaitForCompletion(uploadInfo, _options.AthleteId).ConfigureAwait(false);
    }
}
