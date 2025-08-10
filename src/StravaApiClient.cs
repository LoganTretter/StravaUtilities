namespace StravaUtilities;

/// <summary>
///  
/// </summary>
public partial class StravaApiClient
{
    private const string BaseUrl = "https://www.strava.com";
    private const string ApiPath = "api/v3";

    private string _clientId;
    private string _clientSecret;

    private HttpClient _httpClient;

    public StravaApiClient()
    {
        var baseUri = new Uri(BaseUrl, UriKind.Absolute);
        var apiUri = new Uri(ApiPath + "/", UriKind.Relative);
        _httpClient = new() { BaseAddress = new Uri(baseUri, apiUri) };
    }
}

