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
    /// <para>Creates a new instance of the api client with no existing token or way to look one up.</para>
    /// <para>Subsequent calls will fail unless a token is manually added with <see cref="SetAuthToken"/>, or auth is initiated with <see cref="ExchangeInitialAuthCodeForToken">.</para>
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
    /// Creates a new instance of the api client with an existing token.
    /// On subsequent calls, that token will be used directly.
    /// Any refreshes to the token will be present on the <see cref="Token"/> property.
    /// </summary>
    /// <param name="clientId">The client id for the API application</param>
    /// <param name="clientSecret">The client secret for the API application</param>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException">If any argument is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="clientId"/> or <paramref name="clientSecret"/> are empty or whitespace</exception>
    /// <exception cref="ArgumentException">If <see cref="StravaApiToken.AccessToken"/> is empty or whitespace</exception>
    public StravaApiClient(string clientId, string clientSecret, StravaApiToken token)
        : this(clientId, clientSecret)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (string.IsNullOrWhiteSpace(token.AccessToken))
            throw new ArgumentException($"{nameof(StravaApiToken.AccessToken)} was empty or whitespace in the provided token", nameof(token));

        Token = token;
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
