namespace StravaUtilities;

public class StravaApiToken
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset? AccessTokenExpiration { get; set; }
}