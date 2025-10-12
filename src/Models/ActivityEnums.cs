namespace StravaUtilities;

/// <summary>
/// The data format of an activity
/// </summary>
public enum DataFormat
{
    Fit,
    FitGZipped,
    Tcx,
    TcxGZipped,
    Gpx,
    GpxGZipped
}

/// <summary>
/// The activity/sport type
/// </summary>
public enum ActivityType
{
    AlpineSki,
    BackcountrySki,
    Badminton,
    Canoeing,
    Crossfit,
    EBikeRide,
    Elliptical,
    EMountainBikeRide,
    Golf,
    GravelRide,
    Handcycle,
    HighIntensityIntervalTraining,
    Hike,
    IceSkate,
    InlineSkate,
    Kayaking,
    Kitesurf,
    MountainBikeRide,
    NordicSki,
    Pickleball,
    Pilates,
    Racquetball,
    Ride,
    RockClimbing,
    RollerSki,
    Rowing,
    Run,
    Sail,
    Skateboard,
    Snowboard,
    Snowshoe,
    Soccer,
    Squash,
    StairStepper,
    StandUpPaddling,
    Surfing,
    Swim,
    TableTennis,
    Tennis,
    TrailRun,
    Velomobile,
    VirtualRide,
    VirtualRow,
    VirtualRun,
    Walk,
    WeightTraining,
    Wheelchair,
    Windsurf,
    Workout,
    Yoga
}

/// <summary>
/// The workout type
/// </summary>
public enum WorkoutType
{
    // TODO - figure out the rest of the types
    UnknownValue = -2,
    Default = -1,
    Race = 1,
    LongRun = 2,
    RunWorkout = 3
}
