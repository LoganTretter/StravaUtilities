using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class ActivityTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal async Task GetActivityThatExists()
    {
        long activityId = _options.ActivityIdToGet;

        var activity = await stravaApiClient.GetActivity(activityId, _options.AthleteId).ConfigureAwait(false);

        ;
    }

    internal async Task GetActivityThatDoesNotExist()
    {
        try
        {
            long activityId = 0;

            var activity = await stravaApiClient.GetActivity(activityId, _options.AthleteId).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ;
        }
    }

    internal async Task GetActivitiesInPages()
    {
        var firstTwoActivities = await stravaApiClient.GetActivities(_options.AthleteId, pageSize: 2, pageNumber: 1).ConfigureAwait(false);
        var secondTwoActivities = await stravaApiClient.GetActivities(_options.AthleteId, pageSize: 2, pageNumber: 2).ConfigureAwait(false);
        ;
    }

    internal async Task UpdateActivity()
    {
        var activityBefore = await stravaApiClient.GetActivity(_options.ActivityIdToUpdate, _options.AthleteId).ConfigureAwait(false);

        var activityUpdateInfo = new ActivityUpdateInfo
        {
            ActivityId = _options.ActivityIdToUpdate,
            Name = $"Updated Name at {DateTime.UtcNow.ToString("O")}",
            Description = $"updated description at {DateTime.UtcNow.ToString("O")}",
            SportType = ActivityType.Run,
            DeviceName = "Test Device",
            Trainer = false,
            Commute = false,
            WorkoutType = WorkoutType.RunWorkout,
            SuppressFromFeed = true
        };

        var activityAfter = await stravaApiClient.UpdateActivity(activityUpdateInfo, _options.AthleteId).ConfigureAwait(false);

        ;
    }
}
