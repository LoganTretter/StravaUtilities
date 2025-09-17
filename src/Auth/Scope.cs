namespace StravaUtilities;

/// <summary>
/// Represents the various levels of access permissions that can be granted to an application
/// </summary>
public enum Scope
{
    /// <summary>
    /// read public segments, public routes, public profile data, public posts, public events, club feeds, and leaderboards
    /// </summary>
    Read,
    /// <summary>
    /// read private routes, private segments, and private events for the user
    /// </summary>
    ReadAll,
    /// <summary>
    /// read all profile information even if the user has set their profile visibility to Followers or Only You
    /// </summary>
    ProfileReadAll,
    /// <summary>
    /// update the user's weight and Functional Threshold Power (FTP), and access to star or unstar segments on their behalf
    /// </summary>
    ProfileWrite,
    /// <summary>
    /// read the user's activity data for activities that are visible to Everyone and Followers, excluding privacy zone data
    /// </summary>
    ActivityRead,
    /// <summary>
    /// the same access as activity:read, plus privacy zone data and access to read the user's activities with visibility set to Only You
    /// </summary>
    ActivityReadAll,
    /// <summary>
    /// access to create manual activities and uploads, and access to edit any activities that are visible to the app, based on activity read access level
    /// </summary>
    ActivityWrite
}

internal static class ScopeExtensions
{
    internal static string ToStravaStringRepresentation(this Scope scope)
    {
        return scope switch
        {
            Scope.Read => "read",
            Scope.ReadAll => "read_all",
            Scope.ProfileReadAll => "profile:read_all",
            Scope.ProfileWrite => "profile:write",
            Scope.ActivityRead => "activity:read",
            Scope.ActivityReadAll => "activity:read_all",
            Scope.ActivityWrite => "activity:write",
            _ => throw new NotImplementedException()
        };
    }

    internal static Scope FromStravaStringRepresentation(string scopeString)
    {
        return scopeString switch
        {
            "read" => Scope.Read,
            "read_all" => Scope.ReadAll,
            "profile:read_all" => Scope.ProfileReadAll,
            "profile:write" => Scope.ProfileWrite,
            "activity:read" => Scope.ActivityRead,
            "activity:read_all" => Scope.ActivityReadAll,
            "activity:write" => Scope.ActivityWrite,
            _ => throw new NotImplementedException($"Scope string '{scopeString}' is not mapped to an enum value")
        };
    }
}
