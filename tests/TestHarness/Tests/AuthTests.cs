using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class AuthTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient, IStravaApiAthleteAuthInfoStorer stravaApiTokenStorer)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal async Task PromptUserToAuthorizeAndCompleteAuthProcess()
    {
        await stravaApiClient.PromptUserToAuthorizeAndCompleteAuthProcess([Scope.Read, Scope.ReadAll, Scope.ProfileReadAll, Scope.ActivityRead, Scope.ActivityReadAll, Scope.ActivityWrite]);
    }

    internal void PromptUserToAuthorize()
    {
        stravaApiClient.PromptUserToAuthorize([Scope.Read, Scope.ReadAll, Scope.ProfileReadAll, Scope.ActivityRead, Scope.ActivityReadAll, Scope.ActivityWrite]);
    }

    internal async Task ExchangeInitialAuthCodeForToken()
    {
        var exchangeTokenResponse = new ExchangeTokenInfo
        {
            AuthorizationCode = "insert-code-here",
            Scopes = [Scope.Read, Scope.ReadAll, Scope.ProfileReadAll, Scope.ActivityRead, Scope.ActivityReadAll, Scope.ActivityWrite]
        };

        await stravaApiClient.ExchangeInitialAuthCodeForToken(exchangeTokenResponse);
    }

    internal async Task ClientWithoutAuthInfoOrStorerWillFailWithoutProvidingAuthOnEachCall()
    {
        // Here I'm assuming my token storer has auth info for my athlete
        // Using the constructor that doesn't provide a storer, any subsequent call will fail

        using var client = new StravaApiClient(_options.StravaApiClientId, _options.StravaApiClientSecret);

        try
        {
            var athlete = await client.GetAthlete(_options.AthleteId);
        }
        catch (Exception ex)
        {
            ; // fails, expectedly
        }

        // Now if I manually get the token (that I assume is in the store) and use it directly on the call, it should succeed

        var existingAuthInfo = await stravaApiTokenStorer.GetAthleteAuthInfo(_options.AthleteId);

        try
        {
            var athlete = await client.GetAthlete(_options.AthleteId, existingAuthInfo);
            ; // should succeed
        }
        catch (Exception ex)
        {
            ;
        }
    }

    internal async Task ClientWithStorerCanLookupAuthAndSucceed()
    {
        // Here I'm assuming my token storer has auth info for my athlete
        // Using the constructor that a storer, any subsequent call should make it look up the auth info

        using var client = new StravaApiClient(_options.StravaApiClientId, _options.StravaApiClientSecret, stravaApiTokenStorer);

        try
        {
            var athlete = await client.GetAthlete(_options.AthleteId);
            ; // should succeed
        }
        catch (Exception ex)
        {
            ;
        }
    }
}
