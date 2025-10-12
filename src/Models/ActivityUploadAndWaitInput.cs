namespace StravaUtilities;

/// <summary>
/// Input for an activity upload, that has additional fields compared to plain upload, that need to be sent as a follow-up update
/// </summary>
public class ActivityUploadAndWaitInput : ActivityUploadInput
{
    // The initial upload function doesn't support setting these field (that I could figure out anyway)
    // So they must be sent in an update

    public string? GearId { get; set; }
    public bool SuppressFromFeed { get; set; }
}

/// <summary>
/// Input for an activity upload from a byte array
/// </summary>
public class ActivityUploadAndUpdateFromBytes : ActivityUploadAndWaitInput, IActivityUploadFromBytes
{
    public required byte[] ActivityFileBytes { get; set; }
}

/// <summary>
/// Input for an activity upload from a file
/// </summary>
public class ActivityUploadAndUpdateFromFile : ActivityUploadAndWaitInput, IActivityUploadFromFile
{
    public required string ActivityFilePath { get; set; }
}

/// <summary>
/// Input for an activity upload from a string
/// </summary>
public class ActivityUploadAndUpdateFromString : ActivityUploadAndWaitInput, IActivityUploadFromString
{
    public required string ActivityFileString { get; set; }
}
