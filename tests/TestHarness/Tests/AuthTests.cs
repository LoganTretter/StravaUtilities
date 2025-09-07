using Microsoft.Extensions.Options;

namespace StravaUtilities.TestHarness.Tests;

internal class AuthTests(IOptions<StravaUtilitiesTestHarnessOptions> options, StravaApiClient stravaApiClient, IStravaApiTokenStorer stravaApiTokenStorer)
    : IStravaUtilitiesTest
{
    private readonly StravaUtilitiesTestHarnessOptions _options = options.Value;

    internal void PromptUserToAuthorize()
    {
        stravaApiClient.PromptUserToAuthorize([Scope.Read, Scope.ReadAll, Scope.ProfileReadAll, Scope.ActivityRead, Scope.ActivityReadAll, Scope.ActivityWrite]);
    }

    internal async Task ExchangeInitialAuthCodeForToken()
    {
        await stravaApiClient.ExchangeInitialAuthCodeForToken("insert-code-here");
    }

    internal async Task ClientWithoutTokenOrStorerWillFail()
    {
        // Here I'm assuming my token storer has a token
        // Using the constructor that doesn't provide either a token or a storer, any subsequent call will fail

        using var client = new StravaApiClient(_options.StravaApiClientId, _options.StravaApiClientSecret);

        try
        {
            var athlete = await client.GetAthlete();
        }
        catch (Exception ex)
        {
            ; // fails, expectedly
        }

        // Now if I manually get the token (that I assume is in the store) and set it, subsequent calls succeed

        var existingToken = await stravaApiTokenStorer.GetToken();

        client.SetAuthToken(existingToken);

        try
        {
            var athlete = await client.GetAthlete();
            ; // should succeed
        }
        catch (Exception ex)
        {
            ;
        }
    }

    internal async Task ClientWithTokenConstructorSucceeds()
    {
        // Here again I assume my token storer has a token
        // Using the constructor that takes a token directly, subsequent calls succeed

        var existingToken = await stravaApiTokenStorer.GetToken();

        using var client = new StravaApiClient(_options.StravaApiClientId, _options.StravaApiClientSecret, existingToken);

        try
        {
            var athlete = await client.GetAthlete();
            ; // should succeed
        }
        catch (Exception ex)
        {
            ;
        }
    }
}
