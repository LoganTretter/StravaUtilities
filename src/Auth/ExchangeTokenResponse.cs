using System.Text.Json.Serialization;

namespace StravaUtilities;

/// <summary>
/// A class holding exchange token info from Strava after a user authorizes the app
/// </summary>
internal class ExchangeTokenResponse
{
    public ExchangeTokenResponse() { }

    /// <summary>
    /// Builds a new exchange token response based on the query param key value pairs given by the Strava authorization redirect
    /// </summary>
    /// <param name="queryParamKeyValuePairs"></param>
    /// <exception cref="StravaUtilitiesException"></exception>
    public ExchangeTokenResponse(Dictionary<string, string> queryParamKeyValuePairs)
    {
        if (queryParamKeyValuePairs.TryGetValue("code", out var authorizationCode))
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
                throw new StravaUtilitiesException("Authorization code on the Strava authorization redirect was null or whitespace");

            AuthorizationCode = authorizationCode.ToString();
        }
        else
        {
            throw new StravaUtilitiesException("Authorization code was not provided by the Strava authorization redirect");
        }

        if (queryParamKeyValuePairs.TryGetValue("scope", out var scopeString))
        {
            if (string.IsNullOrWhiteSpace(scopeString))
                throw new StravaUtilitiesException("Scope on the Strava authorization redirect was null or whitespace");

            ScopesDelimited = scopeString.ToString();
        }
        else
        {
            throw new StravaUtilitiesException("Scope was not provided by the Strava authorization redirect");
        }
    }

    [JsonPropertyName("code")]
    internal string AuthorizationCode { get; set; } = "";
    [JsonPropertyName("scope")]
    internal string ScopesDelimited { get; set; } = "";

    internal List<Scope> Scopes
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ScopesDelimited))
                throw new StravaUtilitiesException("Scope was not provided by the Strava authorization redirect");

            var scopes = new List<Scope>();

            var scopeSegments = ScopesDelimited.Split(',');

            foreach (var scopeSegment in scopeSegments)
            {
                scopes.Add(ScopeExtensions.FromStravaStringRepresentation(scopeSegment));
            }

            return scopes;
        }
    }
}
