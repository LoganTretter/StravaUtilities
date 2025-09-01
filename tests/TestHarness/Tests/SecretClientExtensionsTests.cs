using Azure.Security.KeyVault.Secrets;

namespace StravaUtilities.TestHarness.Tests;

internal class SecretClientExtensionsTests(SecretClient secretClient)
    : IStravaUtilitiesTest
{
    internal async Task GetSecretThatExists()
    {
        var secret = await secretClient.GetSecretValue<string>("StravaApiToken").ConfigureAwait(false);
        ;
    }

    internal async Task GetSecretThatDoesNotExist()
    {
        try
        {
            var secret = await secretClient.GetSecretValue<string>("DoesNotExist").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ;
        }
    }
}
