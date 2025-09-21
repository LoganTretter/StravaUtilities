using Microsoft.Extensions.Caching.Memory;

namespace StravaUtilities;

public partial class StravaApiClient
{
    // TODO need to handle authorization too - check scopes for ability to do certain actions
    // Need write access to upload, read access to see activities, etc.

    private const string AuthorizePath = "oauth/authorize";
    private const string TokenPath = "oauth/token";

    private IStravaApiAthleteAuthInfoStorer? _stravaAthleteAuthInfoStorer;

    private Lazy<IMemoryCache> _lazyTokenCache = new(() => new MemoryCache(new MemoryCacheOptions()));
    private IMemoryCache AuthInfoCache => _lazyTokenCache.Value;

    /// <summary>
    /// <para>Tries to get auth info for the athlete, from cache or storage</para>
    /// <para>If the token is expired it tries to refresh it, and then stores that result</para>
    /// </summary>
    /// <param name="athleteId">The id of the athlete to get auth info for</param>
    /// <returns>Active auth info for the athlete</returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task<StravaApiAthleteAuthInfo> GetAthleteAuthInfoAndRefreshIfNeeded(long athleteId)
    {
        StravaApiAthleteAuthInfo? authInfo = null;

        // TODO try make this thread safe

        if (AuthInfoCache.TryGetValue(athleteId, out var cachedValue) && cachedValue is StravaApiAthleteAuthInfo cachedAuthInfo)
        {
            authInfo = cachedAuthInfo;

            if (authInfo.TokenInfo == null) // unexpected since this is in complete control of the cache, but check anyway
                throw new StravaUtilitiesException($"Auth info was fetched from the cache, but it does not include any {nameof(StravaApiAthleteAuthInfo.TokenInfo)}");
        }
        else if (_stravaAthleteAuthInfoStorer != null)
        {
            var tokenFromStorage = await TryGetAuthInfoFromStorage(athleteId).ConfigureAwait(false);
            
            if (tokenFromStorage == null)
                throw new StravaUtilitiesException($"No token is present, and one was not able to be fetched from the {nameof(IStravaApiAthleteAuthInfoStorer)}");

            authInfo = tokenFromStorage;

            if (authInfo.TokenInfo == null)
                throw new StravaUtilitiesException($"Auth info was fetched from the {nameof(IStravaApiAthleteAuthInfoStorer)}, but it does not include any {nameof(StravaApiAthleteAuthInfo.TokenInfo)}");

            if (string.IsNullOrEmpty(authInfo.TokenInfo.AccessToken))
                throw new StravaUtilitiesException($"Auth info was fetched from the {nameof(IStravaApiAthleteAuthInfoStorer)}, but the {nameof(StravaApiTokenInfo.AccessToken)} was null or empty");
        }
        else
        {
            throw new StravaUtilitiesException($"No token is present, and there is no {nameof(IStravaApiAthleteAuthInfoStorer)} with which to try fetch one");
        }

        var currentTokenInfo = authInfo.TokenInfo;
        if (TokenIsExpired(currentTokenInfo))
        {
            if (string.IsNullOrEmpty(currentTokenInfo.RefreshToken))
                throw new StravaUtilitiesException("Authentication is expired and no refresh token is present to try refresh");

            var newTokenInfo = await GetNewTokenInfoFromRefreshToken(currentTokenInfo.RefreshToken).ConfigureAwait(false);

            if (!newTokenInfo.Equals(currentTokenInfo))
            {
                authInfo.TokenInfo = newTokenInfo;
                await TryAddOrUpdateAthleteAuthInfoToCacheAndStorage(authInfo).ConfigureAwait(false);
            }
        }

        return authInfo;
    }

    /// <summary>
    /// <para>Gets new token info from a refresh token</para>
    /// <para>If a new refresh token is issued, the old one is considered invalid</para>
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public async Task<StravaApiTokenInfo> GetNewTokenInfoFromRefreshToken(string refreshToken)
    {
        // Strava docs say the same token info may be returned if the existing access token does not expire for more than an hour
        // Otherwise it will return new info

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

            var start = DateTimeOffset.UtcNow;

            var authResponse = await StravaHttpClient.Post<TokenResponse>(TokenPath, content: content).ConfigureAwait(false);

            if (string.IsNullOrEmpty(authResponse.AccessToken))
                throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(TokenResponse.AccessToken)} was null or empty");
            if (string.IsNullOrEmpty(authResponse.RefreshToken))
                throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(TokenResponse.RefreshToken)} was null or empty");
            if (authResponse.ExpiresAtSecondsSinceEpoch <= 0)
                throw new StravaUtilitiesException($"Auth call succeeded but response {nameof(TokenResponse.ExpiresAtSecondsSinceEpoch)} was not greater than 0");

            var newTokenInfo = new StravaApiTokenInfo()
            {
                AccessToken = authResponse.AccessToken,
                RefreshToken = authResponse.RefreshToken,
                AccessTokenExpiration = DateTimeOffset.FromUnixTimeSeconds(authResponse.ExpiresAtSecondsSinceEpoch)
            };

            return newTokenInfo;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Auth call failed", ex);
        }
    }

    /// <summary>
    /// <para>Checks whether the token is expired</para>
    /// <para>Always returns true if the token info does not specify an expiration time, it is indeterminate so it assumes expired</para>
    /// </summary>
    /// <param name="tokenInfo"></param>
    /// <returns></returns>
    private bool TokenIsExpired(StravaApiTokenInfo tokenInfo)
    {
        // Consider it expired if it will be in the next minute, to avoid a race condition (auth is validated, then auth expires, then next call is made and fails)
        return tokenInfo.AccessTokenExpiration == null || tokenInfo.AccessTokenExpiration <= DateTimeOffset.UtcNow.AddMinutes(1);
    }

    /// <summary>
    /// Tries to get auth info from storage
    /// </summary>
    /// <param name="athleteId">The id of the athlete the auth info is for</param>
    /// <returns></returns>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task<StravaApiAthleteAuthInfo?> TryGetAuthInfoFromStorage(long athleteId)
    {
        if (_stravaAthleteAuthInfoStorer == null)
            throw new StravaUtilitiesException($"There is no {nameof(IStravaApiAthleteAuthInfoStorer)} configured so auth info cannot be fetched from storage");

        try
        {
            var authInfo = await _stravaAthleteAuthInfoStorer.GetAthleteAuthInfo(athleteId).ConfigureAwait(false);
            return authInfo;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Tried to get token from storage but an exception was thrown", ex);
        }
    }

    /// <summary>
    /// Adds/updates the athlete auth info to the cache and tries to add/update to storage
    /// </summary>
    /// <returns></returns>
    /// <param name="authInfo">The auth info to store</param>
    /// <exception cref="StravaUtilitiesException"></exception>
    private async Task TryAddOrUpdateAthleteAuthInfoToCacheAndStorage(StravaApiAthleteAuthInfo authInfo)
    {
        AuthInfoCache.Set(authInfo.AthleteId, authInfo);

        if (_stravaAthleteAuthInfoStorer == null)
            return;

        try
        {
            await _stravaAthleteAuthInfoStorer.AddOrUpdateAthleteAuthInfo(authInfo).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Tried to add or update token to storage but an exception was thrown", ex);
        }
    }
}
