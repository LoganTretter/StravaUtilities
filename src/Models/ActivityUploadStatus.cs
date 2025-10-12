using System.Text.Json.Serialization;

namespace StravaUtilities;

/// <summary>
/// Info about the status of an activity upload
/// </summary>
public class ActivityUploadStatus
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; set; }
    [JsonPropertyName("error")]
    public string? ErrorMessage { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    [JsonPropertyName("activity_id")]
    public long? ActivityId { get; set; }

    public CurrentUploadStatus CurrentStatus => Status switch
    {
        // They just send back these certain strings, so map them to an enum
        null => CurrentStatus,
        "Your activity is still being processed." => CurrentUploadStatus.Processing,
        "The created activity has been deleted." => CurrentUploadStatus.Deleted,
        "There was an error processing your activity." => CurrentUploadStatus.Error,
        _ => CurrentUploadStatus.Ready,
    };
}

/// <summary>
/// Statuses of an activity upload
/// </summary>
public enum CurrentUploadStatus
{
    Processing,
    Deleted,
    Error,
    Ready
}
