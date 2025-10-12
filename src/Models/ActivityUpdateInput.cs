namespace StravaUtilities;

/// <summary>
/// Input for an activity update
/// </summary>
public class ActivityUpdateInfo
{
    public long ActivityId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ActivityType? SportType { get; set; }
    public string? GearId { get; set; }
    public bool? Trainer { get; set; }
    public bool? Commute { get; set; }
    public WorkoutType? WorkoutType { get; set; } // TODO is this an enum? 3 for run workout is all I know
    public bool? SuppressFromFeed { get; set; }

    // These don't seem to work through the API unfortunately
    //public bool? Private { get; set; } // Doesn't work?
    //public int? Effort { get; set; } // Doesn't work?
}