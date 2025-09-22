namespace StravaUtilities;

/// <summary>
/// A client for interfacing with the Strava API
/// </summary>
public partial class StravaApiClient : IDisposable
{
    private const string BaseUrl = "https://www.strava.com";
    private const string ApiPath = "api/v3";
    private static readonly Uri BaseUri = new Uri(BaseUrl, UriKind.Absolute);

    private readonly Lazy<HttpClient> _lazyHttpClient = new(() => new() { BaseAddress = new Uri(BaseUrl, UriKind.Absolute) });
    private HttpClient StravaHttpClient => _lazyHttpClient.Value;

    private readonly string _clientId;
    private readonly string _clientSecret;

    private bool _disposed;

    /// <summary>
    /// <para>Creates a new instance of the api client with no existing token or way to look one up.</para>
    /// <para>Subsequent calls will fail unless a token is manually provided, or auth is initiated with <see cref="ExchangeInitialAuthCodeForToken">.</para>
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
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
    /// <para>Creates a new instance of the api client with a strava token storer.</para>
    /// <para>On subsequent calls, an existing token will be loaded with the storer and used.</para>
    /// <para>Any refreshes to the token will be stored with the storer.</para>
    /// </summary>
    /// <param name="clientId">The client id for the API application</param>
    /// <param name="clientSecret">The client secret for the API application</param>
    /// <param name="stravaTokenStorer">An instace of a strava token storer</param>
    /// <exception cref="ArgumentNullException">If any argument is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="clientId"/> or <paramref name="clientSecret"/> are empty or whitespace</exception>
    public StravaApiClient(string clientId, string clientSecret, IStravaApiAthleteAuthInfoStorer stravaTokenStorer)
        : this(clientId, clientSecret)
    {
        ArgumentNullException.ThrowIfNull(stravaTokenStorer);

        _stravaAthleteAuthInfoStorer = stravaTokenStorer;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            if (_lazyHttpClient.IsValueCreated)
                StravaHttpClient?.Dispose();

            if (_lazyTokenCache.IsValueCreated)
                AuthInfoCache?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
