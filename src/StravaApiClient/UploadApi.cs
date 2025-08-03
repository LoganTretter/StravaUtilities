using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using StravaUtilities.Models;

namespace StravaUtilities.ApiClient;
public partial class StravaApiClient
{
    // TODO - a callback for updating a status indicator?
    public async Task<ActivityUploadStatus> UploadActivity(ActivityUploadInfo uploadInfo)
    {
        string dataType = uploadInfo.SourceDataFormat switch
        {
            DataFormat.Fit => "fit",
            DataFormat.FitGZipped => "fit.gz",
            DataFormat.Gpx => "gpx",
            DataFormat.GpxGZipped => "gpx.gz",
            DataFormat.Tcx => "tcx",
            DataFormat.TcxGZipped => "tcx.gz",
            _ => throw new ApplicationException("Unsupported source data format.")
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

        using var multipartFormDataContent = new MultipartFormDataContent
        {
            // Couldn't get this to work from the string directly, only from a file
            //{ new ByteArrayContent(Encoding.UTF8.GetBytes(uploadInfo.ActivityFileString)), "file", uploadInfo.ExternalId}
            { new ByteArrayContent(File.ReadAllBytes(uploadInfo.SourceFilePath)), "file", uploadInfo.ExternalId}
        };
        foreach (var (key, val) in vals)
        {
            multipartFormDataContent.Add(new StringContent(val), key);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

        try
        {
            var uploadStatus = await _httpClient.Post<ActivityUploadStatus>("uploads", multipartFormDataContent).ConfigureAwait(false);

            int i = 0;
            while (i++ < 60)
            {
                if (uploadStatus.CurrentStatus == CurrentUploadStatus.Error || !string.IsNullOrEmpty(uploadStatus.ErrorMessage))
                {
                    // Doesn't seem to be possible to delete an activity, or I don't have the write scope
                    //if (reUploadIfDuplicate && (uploadStatus.ErrorMessage?.Contains("duplicate of <a href") ?? false))
                    //{
                    //    long activityId = 0;
                    //    await DeleteActivity(activityId).ConfigureAwait(false);
                    //    return await UploadActivity(uploadInfo, false).ConfigureAwait(false);
                    //}

                    throw new ApplicationException($"Upload status is error:{Environment.NewLine}{uploadStatus.ErrorMessage}");
                }

                if (uploadStatus.CurrentStatus == CurrentUploadStatus.Ready)
                {
                    if (!uploadStatus.ActivityId.HasValue)
                        throw new ApplicationException("Upload status is ready but no activity id was returned. It may or may not be uploaded.");

                    break;
                }

                await Task.Delay(1000).ConfigureAwait(false);

                uploadStatus = await CheckUploadStatus(uploadStatus.Id).ConfigureAwait(false);
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

            await UpdateActivity(updateInfo).ConfigureAwait(false);

            return uploadStatus;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Activity upload error:{Environment.NewLine}{ex.Message}", ex);
        }
    }

    public async Task<ActivityUploadStatus> CheckUploadStatus(long uploadId)
    {
        try
        {
            return await _httpClient.Get<ActivityUploadStatus>($"uploads/{uploadId}").ConfigureAwait(false);
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