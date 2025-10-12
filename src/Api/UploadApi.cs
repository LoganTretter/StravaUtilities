using System.Net.Mime;
using System.Text;

namespace StravaUtilities;

public partial class StravaApiClient
{
    /// <summary>
    /// Uploads an activity to Strava
    /// </summary>
    /// <param name="uploadInfo">Info about the upload</param>
    /// <param name="athleteId">The id of the athlete to upload the activity for</param>
    /// <param name="authInfo">Auth info for the athlete if already present</param>
    /// <returns>The status of the upload</returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    public async Task<ActivityUploadStatus> UploadActivity(ActivityUploadInput uploadInfo, long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
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
            { "trainer", uploadInfo.Trainer.ToString().ToLower() },
            { "commute", uploadInfo.Commute.ToString().ToLower() },
            { "external_id", uploadInfo.ExternalId },

            // Doesn't seem to work, takes value from the file
            //{ "device_name", uploadInfo.DeviceName },

            // Doesn't work - need to send a follow-up update
            //{ "hide_from_home", uploadInfo.SuppressFromFeed.ToString().ToLower() }
        };
        if (uploadInfo.WorkoutType.HasValue)
            vals.Add("workout_type", ((int)uploadInfo.WorkoutType).ToString());

        // Doesn't work - need to send a follow-up update
        //if (!string.IsNullOrWhiteSpace(uploadInfo.GearId))
        //    vals.Add("gear_id", uploadInfo.GearId);

        byte[] fileBytes;
        if (uploadInfo is IActivityUploadFromBytes uploadFromBytes)
        {
            fileBytes = uploadFromBytes.ActivityFileBytes;
        }
        else if (uploadInfo is IActivityUploadFromFile uploadFromFile)
        {
            try
            {
                fileBytes = await File.ReadAllBytesAsync(uploadFromFile.ActivityFilePath).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string msg = "Failed to read activity file";
                throw new StravaUtilitiesException(msg, innerException: ex);
            }
        }
        else if (uploadInfo is IActivityUploadFromString uploadFromString)
        {
            try
            {
                fileBytes = Encoding.UTF8.GetBytes(uploadFromString.ActivityFileString);
            }
            catch (Exception ex)
            {
                string msg = "Failed to convert file string to byte array";
                throw new StravaUtilitiesException(msg, innerException: ex);
            }
        }
        else
        {
            throw new StravaUtilitiesException("Unsupported activity file source");
        }

        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(fileBytes), "file", uploadInfo.ExternalId}
        };

        foreach (var (key, val) in vals)
            content.Add(new StringContent(val), key);

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
    /// <summary>
    /// Uploads an activity to Strava, and waits some time for it to complete (or error) before returning
    /// </summary>
    /// <param name="uploadInfo">Info about the upload</param>
    /// <param name="athleteId">The id of the athlete to upload the activity for</param>
    /// <param name="secondsToWait">How many seconds to wait for the upload to either finish or error</param>
    /// <param name="authInfo">Auth info for the athlete if already present</param>
    /// <returns>The status of the upload</returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ActivityUploadStatus> UploadActivityAndWaitForCompletion(ActivityUploadAndWaitInput uploadInfo, long athleteId, byte secondsToWait = 60, StravaApiAthleteAuthInfo? authInfo = null)
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

                uploadStatus = await GetUploadStatus(uploadStatus.Id, athleteId, authInfo).ConfigureAwait(false);
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

            // Some fields can't be provided in the initial upload, so send them with an update
            if (uploadInfo.SuppressFromFeed || !string.IsNullOrWhiteSpace(uploadInfo.GearId))
            {
                var updateInfo = new ActivityUpdateInfo
                {
                    ActivityId = uploadStatus.ActivityId!.Value,
                    GearId = uploadInfo.GearId,
                    SuppressFromFeed = uploadInfo.SuppressFromFeed
                };
                await UpdateActivity(updateInfo, athleteId, authInfo).ConfigureAwait(false);
            }

            return uploadStatus;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Activity upload error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets the status of an upload
    /// </summary>
    /// <param name="uploadId">The id the upload</param>
    /// <param name="athleteId">The id of the athlete to upload the activity for</param>
    /// <param name="authInfo">Auth info for the athlete if already present</param>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    public async Task<ActivityUploadStatus> GetUploadStatus(long uploadId, long athleteId, StravaApiAthleteAuthInfo? authInfo = null)
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
