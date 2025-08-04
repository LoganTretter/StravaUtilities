using System.Text.Json.Serialization;

namespace StravaApi;

public class Activity
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("resource_state")]
    public ResourceState ResourceState { get; set; }

    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; }

    [JsonPropertyName("upload_id")]
    public long? UploadId { get; set; }

    [JsonPropertyName("athlete")]
    internal MetaAthlete Athlete { get; set; }
    public long AthleteId => Athlete?.Id ?? 0;

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("distance")]
    public decimal Distance { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTimeSeconds { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedSeconds { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public decimal TotalElevationGainMeters { get; set; }

    [JsonPropertyName("elev_high")]
    public decimal HighestElevationMeters { get; set; }

    [JsonPropertyName("elev_low")]
    public decimal LowestElevationMeters { get; set; }

    [JsonPropertyName("sport_type")]
    public ActivityType SportType { get; set; }

    [JsonPropertyName("start_date")]
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Start time in the time zone where the activity started
    /// </summary>
    [JsonPropertyName("start_date_local")]
    public DateTimeOffset StartDateLocal { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    // TODO use a LatitudeLongitude class that separates the two points
    [JsonPropertyName("start_latlng")]
    public decimal[] StartLatitudeLongitude { get; set; }

    [JsonIgnore]
    public LatitudeLongitude StartLocation =>
        StartLatitudeLongitude?.Length == 2 ? new LatitudeLongitude(StartLatitudeLongitude[0], StartLatitudeLongitude[1]) : null;

    [JsonPropertyName("end_latlng")]
    public decimal[] EndLatitudeLongitude { get; set; }

    [JsonIgnore]
    public LatitudeLongitude? EndLocation =>
        EndLatitudeLongitude?.Length == 2 ? new LatitudeLongitude(EndLatitudeLongitude[0], EndLatitudeLongitude[1]) : null;

    [JsonPropertyName("achievement_count")]
    public int AchievementCount { get; set; }

    [JsonPropertyName("kudos_count")]
    public int KudosCount { get; set; }

    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }

    [JsonPropertyName("athlete_count")]
    public int AthleteCount { get; set; }

    /// <summary>
    /// Instagram photo count
    /// </summary>
    [JsonPropertyName("photo_count")]
    public int PhotoCount { get; set; }

    /// <summary>
    /// Instagram and Strava photo count
    /// </summary>
    [JsonPropertyName("total_photo_count")]
    public int TotalPhotoCount { get; set; }

    [JsonPropertyName("map")]
    public PolylineMap Map { get; set; }

    /// <summary>
    /// Whether it is a trainer / treadmill activity
    /// </summary>
    [JsonPropertyName("trainer")]
    public bool Trainer { get; set; }

    [JsonPropertyName("commute")]
    public bool Commute { get; set; }

    [JsonPropertyName("manual")]
    public bool Manual { get; set; }

    [JsonPropertyName("private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("flagged")]
    public bool Flagged { get; set; }

    //[JsonPropertyName("workout_type")]
    //public int WorkoutType { get; set; } // TODO unsure what this means - is it an enum?

    [JsonPropertyName("upload_id_str")]
    public string UploadIdString { get; set; }

    [JsonPropertyName("average_speed")]
    public decimal AverageSpeedMetersPerSecond { get; set; }

    [JsonPropertyName("max_speed")]
    public decimal MaxSpeedMetersPerSecond { get; set; }

    [JsonPropertyName("has_kudoed")]
    public bool CurrentAthleteHasGivenKudos { get; set; }

    [JsonPropertyName("hide_from_home")]
    public bool Muted { get; set; }

    [JsonPropertyName("gear_id")]
    public string GearId { get; set; }

    [JsonPropertyName("kilojoules")]
    public decimal TotalWorkKilojoules { get; set; }

    [JsonPropertyName("average_watts")]
    public decimal AveragePowerWatts { get; set; }

    [JsonPropertyName("device_watts")]
    public bool PowerIsFromMeter { get; set; }

    [JsonPropertyName("max_watts")]
    public int MaxPowerWatts { get; set; }

    [JsonPropertyName("weighted_average_watts")]
    public int WeightedAveragePowerWatts { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    //[JsonPropertyName("photos")]
    //public PhotosSummary Photos { get; set; }

    [JsonPropertyName("gear")]
    public Gear Gear { get; set; }

    [JsonPropertyName("calories")]
    public decimal Calories { get; set; }

    //[JsonPropertyName("segment_efforts")]
    //public DetailedSegmentEffort segment_efforts { get; set; }

    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; }

    [JsonPropertyName("embed_token")]
    public string EmbedToken { get; set; }

    //[JsonPropertyName("splits_metric")]
    //public Split splits_metric { get; set; }

    //[JsonPropertyName("splits_standard")]
    //public Split splits_standard { get; set; }

    //[JsonPropertyName("laps")]
    //public Lap laps { get; set; }

    //[JsonPropertyName("best_efforts")]
    //public DetailedSegmentEffort best_efforts { get; set; }
}

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

public enum WorkoutType
{
    RunWorkout = 3
}

public record LatitudeLongitude (decimal Latitude, decimal Longitude);

public class ActivityUpdateInfo
{
    public long ActivityId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ActivityType? SportType { get; set; }
    public string GearId { get; set; }
    public string DeviceName { get; set; }
    public bool? Trainer { get; set; }
    public bool? Commute { get; set; }
    public WorkoutType? WorkoutType { get; set; } // TODO is this an enum? 3 for run workout is all I know
    public bool? SuppressFromFeed { get; set; }
    public bool? Private { get; set; } // Doesn't work?
    public int? Effort { get; set; } // Doesn't work?
}