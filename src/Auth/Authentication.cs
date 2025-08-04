using System.Net.Http.Headers;

namespace StravaUtilities;

public partial class StravaApiClient
{
    private const string TokenPath = "oauth/token";

    private HttpClient _authHttpClient;

    public StravaApiToken Token { get; set; }

    // TODO need to check scopes for ability to do certain actions
    // Need write access to upload, read access to see activities, etc.

    /// <summary>
    /// Adds authentication to the api client.
    /// 
    /// At minimum the <see cref="StravaApiToken.AccessToken"/> must be provided. Subsequent api calls may be made with just this,
    /// but then the caller is responsible for validating it is still valid before making subsequent calls.
    /// If <see cref="StravaApiToken.RefreshToken"/> and <see cref="StravaApiToken.AccessTokenExpiration"/> are also provided,
    /// and clientid and clientsecret are provided as well, then the token is automatically checked for expiration
    /// and refreshed, and all further calls to the api client will do the same.
    /// 
    /// This call also fetches the athlete corresponding to the authentication, so <see cref="CurrentAuthenticatedAthlete"/> will be set.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="clientId"></param>
    /// <param name="clientSecret"></param>
    /// <returns>The token that was validated. May be new if the one provided was expired.</returns>
    /// <exception cref="ArgumentException">If <see cref="StravaApiToken.AccessToken"/> is null or empty. Or if <paramref name="clientId"/> or <paramref name="clientSecret"/> is provided but not both.</exception>
    /// <exception cref="StravaApiException">If authentication to Strava fails.</exception>
    public async Task<StravaApiToken> Authenticate(StravaApiToken token, string clientId = null, string clientSecret = null)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        if (string.IsNullOrEmpty(clientId) ^ string.IsNullOrEmpty(clientSecret))
            throw new ArgumentException(nameof(clientId), "clientId and clientSecret should both be present or neither.");

        if (string.IsNullOrEmpty(token.AccessToken))
            throw new ArgumentException("Access token was not provided", nameof(token.AccessToken));

        _clientId = clientId;
        _clientSecret = clientSecret;

        var tokenToUse = token;

        // If provided with an expired token, try to automatically refresh it
        if (TokenIsExpired(token) && !string.IsNullOrEmpty(_clientId) && !string.IsNullOrEmpty(_clientSecret) && !string.IsNullOrEmpty(token.RefreshToken))
        {
            tokenToUse = await TryGetNewTokenFromRefresh(token).ConfigureAwait(false);
        }

        if (string.IsNullOrEmpty(tokenToUse.AccessToken))
            throw new StravaApiException("tokenToUse.AccessToken is null when trying to authenticate.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenToUse.AccessToken);

        await SetAthleteForCurrentAuthenticatedUser().ConfigureAwait(false);

        Token = tokenToUse;
        return tokenToUse;
    }

    private async Task SetAthleteForCurrentAuthenticatedUser()
    {
        await SetCurrentAthlete().ConfigureAwait(false);

        if (CurrentAuthenticatedAthlete == null)
        {
            throw new StravaApiException("Could not authenticate with the provided token.");
        }
    }

    private async Task CheckAuthenticationAndRefreshIfNeeded()
    {
        if (Token == null)
            throw new StravaApiException("No successful authentication is added.");
        
        if (Token.AccessTokenExpiration == null || Token.AccessTokenExpiration > DateTimeOffset.UtcNow.AddMinutes(1))
            return;

        var newToken = await TryGetNewTokenFromRefresh(Token).ConfigureAwait(false);

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.AccessToken);

        await SetAthleteForCurrentAuthenticatedUser().ConfigureAwait(false);

        Token = newToken;
    }

    private async Task<StravaApiToken> TryGetNewTokenFromRefresh(StravaApiToken token)
    {
        if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
            throw new StravaApiException("Authentication is expired and ClientId or ClientSecret is not present to try refresh.");

        if (string.IsNullOrEmpty(token.RefreshToken))
            throw new StravaApiException("Authentication is expired and no refresh token is present to try refresh.");

        return await GetRefreshToken(token.RefreshToken, _clientId, _clientSecret).ConfigureAwait(false);
    }

    public async Task<StravaApiToken> GetRefreshToken(string refreshToken, string clientId, string clientSecret)
    {
        using var dictFormUrlEncoded = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        });

        _authHttpClient ??= new() { BaseAddress = new Uri(BaseUrl) };

        var start = DateTimeOffset.UtcNow;

        var authResponse = await _authHttpClient.Post<StravaAuthResponse>(TokenPath, dictFormUrlEncoded).ConfigureAwait(false);
        
        if (string.IsNullOrEmpty(authResponse.AccessToken))
            throw new StravaApiException($"Auth call succeeded but response {nameof(StravaAuthResponse.AccessToken)} was null or empty");
        if (string.IsNullOrEmpty(authResponse.RefreshToken))
            throw new StravaApiException($"Auth call succeeded but response {nameof(StravaAuthResponse.RefreshToken)} was null or empty");
        if (authResponse.ExpiresAtSecondsSinceEpoch <= 0)
            throw new StravaApiException($"Auth call succeeded but response {nameof(StravaAuthResponse.ExpiresAtSecondsSinceEpoch)} was not greater than 0");

        var newToken = new StravaApiToken()
        {
            AccessToken = authResponse.AccessToken,
            RefreshToken = authResponse.RefreshToken,
            AccessTokenExpiration = DateTimeOffset.UtcNow.AddSeconds(authResponse.ExpiresInSeconds - (DateTimeOffset.UtcNow - start).TotalSeconds)
        };

        return newToken;
    }

    private bool TokenIsExpired(StravaApiToken token)
    {
        return token.AccessTokenExpiration != null && token.AccessTokenExpiration <= DateTimeOffset.UtcNow.AddMinutes(1);
    }
}
