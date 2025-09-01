using System.Net.Http.Headers;

namespace StravaUtilities;

public partial class StravaApiClient
{
    public async Task<Activity> GetActivity(long activityId)
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        try
        {
            var activity = await HttpClient.Get<Activity?>($"activities/{activityId}").ConfigureAwait(false);

            if (activity == null)
                throw new StravaUtilitiesException("Activity call succeeded but result is null.");

            return activity;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Error getting activity by id {activityId}:{Environment.NewLine}{ex.Message}", ex);
        }
    }

    public async Task<List<Activity>> GetActivities(int pageSize = 200, int pageNumber = 1)
    {
        if (pageSize > 200)
            throw new StravaUtilitiesException($"Max {nameof(pageSize)} is 200, received {pageSize}. The default is 200.");

        if (pageSize < 1)
            throw new StravaUtilitiesException($"{nameof(pageSize)} must be >= 1. The default is 200.");

        if (pageNumber < 1)
            throw new StravaUtilitiesException($"{nameof(pageNumber)} must be >= 1. The first page is # 1.");

        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        try
        {
            var activities = await HttpClient.Get<List<Activity>?>($"activities?per_page={pageSize}&page={pageNumber}").ConfigureAwait(false);

            if (activities == null)
                throw new StravaUtilitiesException("Activity call succeeded but result is null.");

            return activities;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Error getting activities:{Environment.NewLine}{ex.Message}", ex);
        }
    }

    public async Task<Activity> UpdateActivity(ActivityUpdateInfo updateInfo)
    {
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

        var contents = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(updateInfo.Name))
            contents.Add("name", updateInfo.Name);
        if (!string.IsNullOrEmpty(updateInfo.Description))
            contents.Add("description", updateInfo.Description);
        if (updateInfo.SportType.HasValue)
            contents.Add("sport_type", updateInfo.SportType.ToString());
        if (!string.IsNullOrEmpty(updateInfo.GearId))
            contents.Add("gear_id", updateInfo.GearId);
        if (!string.IsNullOrEmpty(updateInfo.DeviceName))
            contents.Add("device_name", updateInfo.DeviceName);
        if (updateInfo.Trainer.HasValue)
            contents.Add("trainer", updateInfo.Trainer.ToString().ToLower());
        if (updateInfo.Commute.HasValue)
            contents.Add("commute", updateInfo.Commute.ToString().ToLower());
        if (updateInfo.WorkoutType.HasValue)
            contents.Add("workout_type", ((int)updateInfo.WorkoutType).ToString());
        if (updateInfo.Effort.HasValue)
            contents.Add("perceived_exertion", updateInfo.Effort.ToString()); // TODO this doesn't seem to work

        // TODO workout flag, private notes?

        if (updateInfo.SuppressFromFeed == true) // This is an apparent api bug, like if you include it at all it hides it, rather than respect the value of true/false
            contents.Add("hide_from_home", "true");
        if (updateInfo.Private.HasValue)
            contents.Add("private", updateInfo.Private.ToString().ToLower());

        var formUrlEncodedContents = new FormUrlEncodedContent(contents);

        try
        {
            return await HttpClient.Put<Activity>($"activities/{updateInfo.ActivityId}", formUrlEncodedContents).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Activity update error:{Environment.NewLine}{ex.Message}", ex);
        }
    }

    public async Task DeleteActivity(long activityId)
    {
        await CheckAuthenticationAndRefreshIfNeeded().ConfigureAwait(false);

        try
        {
            await HttpClient.Delete($"activities/{activityId}").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Error deleting activity by id {activityId}:{Environment.NewLine}{ex.Message}", ex);
        }
    }
}