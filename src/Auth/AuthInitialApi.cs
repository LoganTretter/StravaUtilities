namespace StravaUtilities;

public partial class StravaApiClient
{
    /// <summary>
    /// <para>Opens a web page from Strava to prompt the user to authorize this API application.</para>
    /// <para>After the user authorizes, they will be taken to <paramref name="redirectUrl"/>. Embedded in this will be the initial authorization code.</para>
    /// <para>That code can be manually used to call <see cref="ExchangeInitialAuthCodeForToken"/> to complete the authorization flow.</para>
    /// </summary>
    /// <param name="scopes">The scopes to request</param>
    /// <param name="redirectUrl">A url to redirect the user to after authorizing. If no value is provided it will just take them to a blank localhost page.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="StravaUtilitiesException"></exception>
    public void PromptUserToAuthorize(ICollection<Scope> scopes, string redirectUrl = "http://localhost:8080/exchange_token")
    {
        ArgumentNullException.ThrowIfNull(scopes);
        if (scopes.Count == 0)
            throw new ArgumentException("Must provide at least one scope", nameof(scopes));

        if (!Uri.IsWellFormedUriString(redirectUrl, UriKind.Absolute))
            throw new ArgumentException($"The provided redirect url is not a valid uri: '{redirectUrl}'", nameof(redirectUrl));

        var scopesString = string.Join(",", scopes.Select(s => s.ToStravaStringRepresentation()));

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
            LocalServer.OpenWebPage(url);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Failed to open the Strava authorization url", ex);
        }
    }

    /// <summary>
    /// <para>Opens a web page from Strava to prompt the user to authorize this API application.</para>
    /// <para>After the user authorizes, they will be taken to a generic local host page, and this process will complete the auth process.</para>
    /// </summary>
    /// <param name="scopes">The scopes to request</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="StravaUtilitiesException"></exception>
    public async Task<StravaApiAthleteAuthInfo> PromptUserToAuthorizeAndCompleteAuthProcess(ICollection<Scope> scopes)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        if (scopes.Count == 0)
            throw new ArgumentException("Must provide at least one scope", nameof(scopes));

        var redirectUrl = "http://localhost:8080/exchange_token/";

        PromptUserToAuthorize(scopes, redirectUrl);

        using var localServer = new LocalServer(redirectUrl);

        // Wait for the redirect and get the query params from the redirect
        var queryParamKeyValuePairs = await localServer.WaitForRequestAndGetQueryParams(TimeSpan.FromMinutes(1)).ConfigureAwait(false);

        if (queryParamKeyValuePairs.TryGetValue("code", out var authorizationCode))
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
                throw new StravaUtilitiesException("Authorization code on the Strava authorization redirect was null or whitespace");
        }
        else
        {
            throw new StravaUtilitiesException("Authorization code was not provided by the Strava authorization redirect");
        }

        List<Scope> authorizedScopes;
        if (queryParamKeyValuePairs.TryGetValue("scope", out var scopeString))
        {
            if (string.IsNullOrWhiteSpace(scopeString))
                throw new StravaUtilitiesException("Scope on the Strava authorization redirect was null or whitespace");

            authorizedScopes = scopeString.Split(',').Select(s => ScopeExtensions.FromStravaStringRepresentation(s)).ToList();
        }
        else
        {
            throw new StravaUtilitiesException("Scope was not provided by the Strava authorization redirect");
        }

        var exchangeTokenInfo = new ExchangeTokenInfo
        {
            AuthorizationCode = authorizationCode,
            Scopes = authorizedScopes
        };

        return await ExchangeInitialAuthCodeForToken(exchangeTokenInfo).ConfigureAwait(false);
    }

    /// <summary>
    /// <para>Exchanges the initial authorization code for an access token from the Strava API, completing the user authorization flow.</para>
    /// <para>Stores it if a <see cref="IStravaApiAthleteAuthInfoStorer"> is present.</para>
    /// </summary>
    /// <param name="exchangeTokenInfo">Info from Strava when the athlete authorized the app</param>
    /// <returns>A <see cref="StravaApiAthleteAuthInfo"/></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="StravaUtilitiesException"></exception>
    public async Task<StravaApiAthleteAuthInfo> ExchangeInitialAuthCodeForToken(ExchangeTokenInfo exchangeTokenInfo)
    {
        ArgumentNullException.ThrowIfNull(exchangeTokenInfo);
        if (string.IsNullOrWhiteSpace(exchangeTokenInfo.AuthorizationCode))
            throw new ArgumentException($"The {nameof(ExchangeTokenInfo.AuthorizationCode)} was null, empty, or only whitespace, so there is nothing to exchange for a token", nameof(exchangeTokenInfo));


        var start = DateTimeOffset.UtcNow;
        InitialAuthResponse? authResponse;
        try
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "authorization_code" },
                { "code", exchangeTokenInfo.AuthorizationCode }
            });

            authResponse = await StravaHttpClient.Post<InitialAuthResponse>(TokenPath, content: content).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            var msg = "Call to exchange initial auth code for token failed";
            throw new StravaUtilitiesException(msg, innerException: ex);
        }

        if (string.IsNullOrEmpty(authResponse.AccessToken))
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(InitialAuthResponse.AccessToken)} was null or empty");
        if (string.IsNullOrEmpty(authResponse.RefreshToken))
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(InitialAuthResponse.RefreshToken)} was null or empty");
        if (authResponse.ExpiresAtSecondsSinceEpoch <= 0)
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(InitialAuthResponse.ExpiresAtSecondsSinceEpoch)} was not greater than 0");
        if (authResponse.Athlete == null)
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(InitialAuthResponse.Athlete)} was null");
        if (authResponse.Athlete.Id < 0)
            throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(InitialAuthResponse.Athlete)} Id less than 0, indicating it is invalid");

        var newAthleteAuthInfo = new StravaApiAthleteAuthInfo()
        {
            AthleteId = authResponse.Athlete.Id,
            Scopes = exchangeTokenInfo.Scopes,
            TokenInfo = new StravaApiTokenInfo
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                AccessTokenExpiration = DateTimeOffset.FromUnixTimeSeconds(authResponse.ExpiresAtSecondsSinceEpoch)
            }
        };

        await TryAddOrUpdateAthleteAuthInfoToCacheAndStorage(newAthleteAuthInfo).ConfigureAwait(false);

        return newAthleteAuthInfo;
    }
}
