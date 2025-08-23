namespace StravaUtilities;

/// <summary>
/// A client for interfacing with the Strava API
/// </summary>
public partial class StravaApiClient : IDisposable
{
    private const string BaseUrl = "https://www.strava.com";
    private const string ApiPath = "api/v3";
    private static readonly Uri BaseUri = new Uri(BaseUrl, UriKind.Absolute);
    private static readonly Uri ApiUri = new Uri(ApiPath + "/", UriKind.Relative);

    private readonly Lazy<HttpClient> _lazyHttpClient =
        new Lazy<HttpClient>(() => new() { BaseAddress = new Uri(BaseUri, ApiUri) });
    private HttpClient HttpClient => _lazyHttpClient.Value;

    private readonly string _clientId;
    private readonly string _clientSecret;

    private bool _disposed;

    /// <summary>
    /// Creates a new instance of the api client
    /// </summary>
    /// <param name="clientId">The client id for the API application</param>
    /// <param name="clientSecret">The client secret for the API application</param>
    /// <exception cref="ArgumentNullException">If any argument is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="clientId"/> or <paramref name="clientSecret"/> are empty or whitespace</exception>
    public StravaApiClient(string clientId, string clientSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(clientSecret);

        _clientId = clientId;
        _clientSecret = clientSecret;
    }

    /// <summary>
    /// Creates a new instance of the api client with a strava token storer
    /// </summary>
    /// <param name="clientId">The client id for the API application</param>
    /// <param name="clientSecret">The client secret for the API application</param>
    /// <param name="stravaTokenStorer">An instace of a strava token storer</param>
    /// <exception cref="ArgumentNullException">If any argument is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="clientId"/> or <paramref name="clientSecret"/> are empty or whitespace</exception>
    public StravaApiClient(string clientId, string clientSecret, IStravaApiTokenStorer stravaTokenStorer)
        : this(clientId, clientSecret)
    {
        ArgumentNullException.ThrowIfNull(stravaTokenStorer);

        _stravaTokenStorer = stravaTokenStorer;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            if (_lazyAuthHttpClient.IsValueCreated)
                AuthHttpClient?.Dispose();

            if (_lazyHttpClient.IsValueCreated)
                HttpClient?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
