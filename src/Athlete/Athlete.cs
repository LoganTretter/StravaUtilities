using System.Text.Json.Serialization;

namespace StravaApi;

public class MetaAthlete
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}

public class Athlete : MetaAthlete
{
    [JsonPropertyName("resource_state")]
    public ResourceState ResourceState { get; set; }

    [JsonPropertyName("firstname")]
    public string FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string LastName { get; set; }

    [JsonPropertyName("sex")]
    public string Gender { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("weight")]
    public decimal Weight { get; set; }

    [JsonPropertyName("follower_count")]
    public int FollowerCount { get; set; }

    [JsonPropertyName("friend_count")]
    public int FriendCount { get; set; }

    [JsonPropertyName("mutual_friend_count")]
    public int MutualFriendCount { get; set; }

    [JsonPropertyName("date_preference")]
    public string DatePreference { get; set; }

    [JsonPropertyName("measurement_preference")]
    public string MeasurementPreference { get; set; }

    [JsonPropertyName("ftp")]
    public int? FunctionalThresholdPower { get; set; }

    [JsonPropertyName("profile")]
    public string ProfilePictureUrl { get; set; }

    [JsonPropertyName("bikes")]
    public List<Bike> Bikes { get; set; }

    [JsonPropertyName("shoes")]
    public List<Shoe> Shoes { get; set; }

    //[JsonPropertyName("clubs")]
    //public List<Club> Clubs { get; set; }
}