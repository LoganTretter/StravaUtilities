namespace StravaUtilities.TestHarness.Tests;

internal class AuthTests(StravaApiClient stravaApiClient)
    : IStravaUtilitiesTest
{
    internal void PromptUserToAuthorize()
    {
        stravaApiClient.PromptUserToAuthorize([Scope.Read, Scope.ReadAll, Scope.ProfileReadAll, Scope.ActivityRead, Scope.ActivityReadAll, Scope.ActivityWrite]);
    }

    internal async Task ExchangeInitialAuthCodeForToken()
    {
        await stravaApiClient.ExchangeInitialAuthCodeForToken("inser-code-here");
    }
}
