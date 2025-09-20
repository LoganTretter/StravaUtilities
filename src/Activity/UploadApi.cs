using System.Net.Mime;
using System.Text.Json.Serialization;

namespace StravaUtilities;
public partial class StravaApiClient
{
    public async Task<ActivityUploadStatus> UploadActivity(ActivityUploadInfo uploadInfo, long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
    {
        string dataType = uploadInfo.SourceDataFormat switch
        {
            DataFormat.Fit => "fit",
            DataFormat.FitGZipped => "fit.gz",
            DataFormat.Gpx => "gpx",
            DataFormat.GpxGZipped => "gpx.gz",
            DataFormat.Tcx => "tcx",
            DataFormat.TcxGZipped => "tcx.gz",
            _ => throw new StravaUtilitiesException($"Unsupported source data format '{uploadInfo.SourceDataFormat}'")
        };

        var vals = new Dictionary<string, string>
        {
            { "data_type", dataType },
            { "activity_type", uploadInfo.ActivityType.ToString() },
            { "name", uploadInfo.ActivityName },
            { "description", uploadInfo.Description },
            { "private", uploadInfo.Private.ToString().ToLower() },
            { "trainer", uploadInfo.Trainer.ToString().ToLower() },
            { "commute", uploadInfo.Commute.ToString().ToLower() },
            { "external_id", uploadInfo.ExternalId },
            { "device_name", uploadInfo.DeviceName }
        };

        if (uploadInfo.WorkoutType.HasValue)
            vals.Add("workout_type", ((int)uploadInfo.WorkoutType).ToString());

        using var content = new MultipartFormDataContent
        {
            // Couldn't get this to work from the string directly, only from a file
            //{ new ByteArrayContent(Encoding.UTF8.GetBytes(uploadInfo.ActivityFileString)), "file", uploadInfo.ExternalId}
            { new ByteArrayContent(File.ReadAllBytes(uploadInfo.SourceFilePath)), "file", uploadInfo.ExternalId}
        };
        foreach (var (key, val) in vals)
        {
            content.Add(new StringContent(val), key);
        }

        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        try
        {
            var uploadStatus = await StravaHttpClient.Post<ActivityUploadStatus>($"{ApiPath}/uploads", authInfo, content, MediaTypeNames.Multipart.FormData).ConfigureAwait(false);

            return uploadStatus;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Activity upload error: {ex.Message}", ex);
        }
    }

    // TODO - a callback for updating a status indicator?
    public async Task<ActivityUploadStatus> UploadActivityAndWaitForCompletion(ActivityUploadInfo uploadInfo, long athleteId, byte secondsToWait = 60, StravaApiAthleteAuthInfo? authInfo = null)
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        try
        {
            var uploadStatus = await UploadActivity(uploadInfo, athleteId).ConfigureAwait(false);

            ushort i = 0;
            while (i++ < secondsToWait)
            {
                if (uploadStatus.CurrentStatus == CurrentUploadStatus.Error || !string.IsNullOrEmpty(uploadStatus.ErrorMessage))
                {
                    break;
                }

                if (uploadStatus.CurrentStatus == CurrentUploadStatus.Ready)
                {
                    if (!uploadStatus.ActivityId.HasValue)
                        throw new StravaUtilitiesException("Upload status is ready but no activity id was returned. It may or may not be uploaded.");

                    break;
                }

                await Task.Delay(millisecondsDelay: 1000).ConfigureAwait(false);

                uploadStatus = await CheckUploadStatus(uploadStatus.Id, athleteId, authInfo).ConfigureAwait(false);
            }

            switch (uploadStatus.CurrentStatus)
            {
                case CurrentUploadStatus.Deleted:
                    throw new StravaUtilitiesException($"Upload indicates the activity is deleted: {uploadStatus.ErrorMessage}");
                case CurrentUploadStatus.Error:
                    throw new StravaUtilitiesException($"Upload errored: {uploadStatus.ErrorMessage}");
                case CurrentUploadStatus.Processing:
                    string msg = "Upload is still processing" + (secondsToWait > 0 ? $" after waiting {i} seconds" : "");
                    throw new StravaUtilitiesException(msg);
                case CurrentUploadStatus.Ready:
                    break;
                default:
                    throw new NotImplementedException($"{nameof(CurrentUploadStatus)} of {uploadStatus.CurrentStatus} is not supported");
            }

            // Some fields can't be provided in the initial upload so send them with an update
            var updateInfo = new ActivityUpdateInfo
            {
                ActivityId = uploadStatus.ActivityId.Value,
                GearId = uploadInfo.GearId,
                DeviceName = uploadInfo.DeviceName,
                Trainer = uploadInfo.Trainer,
                WorkoutType = uploadInfo.WorkoutType,
                Effort = uploadInfo.Effort,
                SuppressFromFeed = uploadInfo.SuppressFromFeed,
                Private = uploadInfo.Private
            };
            await UpdateActivity(updateInfo, athleteId, authInfo).ConfigureAwait(false);

            return uploadStatus;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Activity upload error: {ex.Message}", ex);
        }
    }

    public async Task<ActivityUploadStatus> CheckUploadStatus(long uploadId, long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
    {
        authInfo ??= await GetAthleteAuthInfoAndRefreshIfNeeded(athleteId).ConfigureAwait(false);

        try
        {
            return await StravaHttpClient.Get<ActivityUploadStatus>($"{ApiPath}/uploads/{uploadId}", authInfo).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Error checking upload status for upload id {uploadId}:{Environment.NewLine}{ex.Message}", ex);
        }
    }
}

public class ActivityUploadStatus
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; }
    [JsonPropertyName("error")]
    public string ErrorMessage { get; set; }
    [JsonPropertyName("status")]
    public string Status { get; set; }
    [JsonPropertyName("activity_id")]
    public long? ActivityId { get; set; }

    public CurrentUploadStatus CurrentStatus => Status switch
    {
        // They just send back these certain strings...
        "Your activity is still being processed." => CurrentUploadStatus.Processing,
        "The created activity has been deleted." => CurrentUploadStatus.Deleted,
        "There was an error processing your activity." => CurrentUploadStatus.Error,
        _ => CurrentUploadStatus.Ready,
    };
}

public class ActivityUploadInfo
{
    public string ActivityFileString { get; set; }
    public string SourceFilePath { get; set; }
    public DataFormat SourceDataFormat { get; set; }
    public string ActivityName { get; set; }
    public string ExternalId { get; set; }
    public ActivityType ActivityType { get; set; }
    public string Description { get; set; }
    public bool Private { get; set; }
    public bool Trainer { get; set; }
    public bool Commute { get; set; }
    public WorkoutType? WorkoutType { get; set; } // TODO is this an enum? 3 for run workout is all I know
    public int Effort { get; set; }
    public string ActivityId { get; set; }
    public string GearId { get; set; }
    public string DeviceName { get; set; }
    public bool SuppressFromFeed { get; set; }
}

public enum DataFormat
{
    Fit,
    FitGZipped,
    Tcx,
    TcxGZipped,
    Gpx,
    GpxGZipped
}

public enum CurrentUploadStatus
{
    Processing,
    Deleted,
    Error,
    Ready
}