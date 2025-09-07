using System.Diagnostics;
using System.Net.Http.Headers;

namespace StravaUtilities;

public partial class StravaApiClient
{
    private const string AuthorizePath = "oauth/authorize";
    private const string TokenPath = "oauth/token";

    private readonly Lazy<HttpClient> _lazyAuthHttpClient =
        new Lazy<HttpClient>(() => new() { BaseAddress = BaseUri });
    private HttpClient AuthHttpClient => _lazyAuthHttpClient.Value;

    private IStravaApiTokenStorer? _stravaTokenStorer;

    public StravaApiToken? Token { get; set; }

    /// <summary>
    /// <para>Opens a web page from Strava to prompt the user to authorize this API application.</para>
    /// <para>After the user authorizes, they will be taken to <paramref name="redirectUrl"/>. Embedded in this will be the initial authorization code.</para>
    /// <para>That code can be manually used to call <see cref="ExchangeInitialAuthCodeForToken"/> to complete the authorization flow.</para>
    /// </summary>
    /// <param name="scopes">The scopes to request</param>
    /// <param name="redirectUrl">A url to redirect the user to after authorizing. If no value is provided it will just take them to a blank localhost page.    /// </param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="StravaUtilitiesException"></exception>
    public void PromptUserToAuthorize(ICollection<Scope> scopes, string redirectUrl = "http://localhost/exchange_token")
    {
        if (scopes.Count == 0)
            throw new ArgumentException("Must provide at least one scope", nameof(scopes));

        var scopesString = string.Join(",", scopes.Select(s =>
            s switch
            {
                Scope.Read => "read",
                Scope.ReadAll => "read_all",
                Scope.ProfileReadAll => "profile:read_all",
                Scope.ProfileWrite => "profile:write",
                Scope.ActivityRead => "activity:read",
                Scope.ActivityReadAll => "activity:read_all",
                Scope.ActivityWrite => "activity:write",
                _ => throw new NotImplementedException()
            }));

        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "response_type", "code" },
            { "redirect_uri", redirectUrl },
            { "approval_prompt", "force" },
            { "scope", scopesString }
        };

        var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={p.Value}"));

        var uriBuilder = new UriBuilder(BaseUri)
        {
            Path = AuthorizePath,
            Query = queryString.ToString()
        };

        var url = uriBuilder.ToString();

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // open the URL in the default browser
            });
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Failed to open the authorization url", ex);
        }
    }

    /// <summary>
    /// Exchanges the initial authorization code for an access token from the Strava API, completing the user authorization flow.
    /// </summary>
    /// <param name="authorizationCode">The authorization code received from the redirect uri on the user authorization prompt.</param>
    /// <returns>A <see cref="StravaApiToken"/></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    public async Task<StravaApiToken> ExchangeInitialAuthCodeForToken(string authorizationCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizationCode);

        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "authorization_code" },
                { "code", authorizationCode}
            });

            Token = await GetToken(content).ConfigureAwait(false);

            await TryAddOrUpdateTokenToStorage().ConfigureAwait(false);

            return Token;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Auth call failed", ex);
        }
    }

    // TODO need to handle authorization too - check scopes for ability to do certain actions
    // Need write access to upload, read access to see activities, etc.

    /// <summary>
    /// Adds authentication to the api client.
    /// This is required once before any calls if an instance of <see cref="IStravaApiTokenStorer"/> was not provided to the api client.
    /// 
    /// At minimum the <see cref="StravaApiToken.AccessToken"/> must be provided. Subsequent api calls may be made with just this,
    /// but then the caller is responsible for validating it is still valid before making subsequent calls.
    /// If <see cref="StravaApiToken.RefreshToken"/> and <see cref="StravaApiToken.AccessTokenExpiration"/> are also provided,
    /// then the token is automatically checked for expiration and refreshed, and all further calls to the api client will do the same.
    /// 
    /// This call also fetches the athlete corresponding to the authentication, so <see cref="CurrentAuthenticatedAthlete"/> will be set.
    /// </summary>
    /// <param name="token"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException">If <see cref="StravaApiToken.AccessToken"/> is null or empty</exception>
    public void SetAuthToken(StravaApiToken token)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (string.IsNullOrEmpty(token.AccessToken))
            throw new ArgumentException($"token.{nameof(StravaApiToken.AccessToken)} was null or empty in the provided token", nameof(token));

        Token = token;
    }

    /// <summary>
    /// Checks if the api client has a current, valid auth token.
    /// If not, tries to either get a token from storage, or refresh the token.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task<Athlete> CheckAuthenticationAndRefreshIfNeeded()
    {
        if (Token == null)
        {
            // Mostly on the first call to this instance of the client, if token has not yet been provided, try get it from storage (if a storer was provided)
            Token = await TryGetTokenFromStorage().ConfigureAwait(false);

            if (Token == null)
                throw new StravaUtilitiesException("No authentication is added.");

            if (string.IsNullOrEmpty(Token.AccessToken))
                throw new StravaUtilitiesException("Access token was null or empty in token fetched from storage");
        }

        bool refreshed = false;
        if (TokenIsExpired(Token))
        {
            if (string.IsNullOrEmpty(Token.RefreshToken))
                throw new StravaUtilitiesException("Authentication is expired and no refresh token is present to try refresh.");

            Token = await GetRefreshToken(Token.RefreshToken).ConfigureAwait(false);

            await TryAddOrUpdateTokenToStorage().ConfigureAwait(false);

            refreshed = true;
        }
        
        if (refreshed == true || CurrentAuthenticatedAthlete == null)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.AccessToken);
            CurrentAuthenticatedAthlete = await SetCurrentAthlete().ConfigureAwait(false);
        }

        return CurrentAuthenticatedAthlete;
    }

    /// <summary>
    /// Gets a new token from a refresh token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<StravaApiToken> GetRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            });

            return await GetToken(content).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Auth call failed", ex);
        }
    }

    private async Task<StravaApiToken> GetToken(FormUrlEncodedContent content)
    {
        var start = DateTimeOffset.UtcNow;

        var authResponse = await AuthHttpClient.Post<StravaAuthResponse>(TokenPath, content).ConfigureAwait(false);

        if (string.IsNullOrEmpty(authResponse.AccessToken))
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(StravaAuthResponse.AccessToken)} was null or empty");
        if (string.IsNullOrEmpty(authResponse.RefreshToken))
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(StravaAuthResponse.RefreshToken)} was null or empty");
        if (authResponse.ExpiresAtSecondsSinceEpoch <= 0)
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(StravaAuthResponse.ExpiresAtSecondsSinceEpoch)} was not greater than 0");

        var newToken = new StravaApiToken()
        {
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            AccessTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(authResponse.ExpiresInSeconds - (DateTimeOffset.UtcNow - start).TotalSeconds)
        };

        return newToken;
    }

    /// <summary>
    /// Checks whether the token is expired
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private bool TokenIsExpired(StravaApiToken token)
    {
        // Consider it expired if it will be in the next minute, to avoid a race condition (auth is validated, then auth expires, then next call is made and fails)
        return token.AccessTokenExpiration == null || token.AccessTokenExpiration <= DateTimeOffset.UtcNow.AddMinutes(1);
    }

    /// <summary>
    /// Tries to get a token from storage
    /// </summary>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task<StravaApiToken?> TryGetTokenFromStorage()
    {
        if (_stravaTokenStorer == null)
            return null;

        try
        {
            var token = await _stravaTokenStorer.GetToken().ConfigureAwait(false);
            return token;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Tried to get token from storage but an exception was thrown", ex);
        }
    }

    /// <summary>
    /// Tries to add or update the current token to storage
    /// </summary>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task TryAddOrUpdateTokenToStorage()
    {
        if (_stravaTokenStorer == null)
            return;

        if (Token == null)
            return;

        try
        {
            await _stravaTokenStorer.AddOrUpdateToken(Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Tried to add or update token to storage but an exception was thrown", ex);
        }
    }
}
