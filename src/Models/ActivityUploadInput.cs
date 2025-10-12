namespace StravaUtilities;

/// <summary>
/// Input for an activity upload
/// </summary>
public class ActivityUploadInput
{
    public DataFormat SourceDataFormat { get; set; }
    public required string ActivityName { get; set; }
    public required string ExternalId { get; set; }
    public required ActivityType ActivityType { get; set; }
    public string Description { get; set; } = "";
    public bool Private { get; set; }
    public bool Trainer { get; set; }
    public bool Commute { get; set; }
    public WorkoutType? WorkoutType { get; set; } // TODO is this an enum? 3 for run workout is all I know
    public int Effort { get; set; }
}

/// <summary>
/// Input for an activity upload from a byte array
/// </summary>
public interface IActivityUploadFromBytes
{
    byte[] ActivityFileBytes { get; set; }
}
public class ActivityUploadFromBytes : ActivityUploadInput, IActivityUploadFromBytes
{
    public required byte[] ActivityFileBytes { get; set; }
}

/// <summary>
/// Input for an activity upload from a file
/// </summary>
public interface IActivityUploadFromFile
{
    string ActivityFilePath { get; set; }
}
public class ActivityUploadFromFile : ActivityUploadInput, IActivityUploadFromFile
{
    public required string ActivityFilePath { get; set; }
}

/// <summary>
/// Input for an activity upload from a string
/// </summary>
public interface IActivityUploadFromString
{
    string ActivityFileString { get; set; }
}
public class ActivityUploadFromString : ActivityUploadInput, IActivityUploadFromString
{
    public required string ActivityFileString { get; set; }
}
