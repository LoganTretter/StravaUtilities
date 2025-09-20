using Azure.Security.KeyVault.Secrets;

namespace StravaUtilities.TestHarness.Tests;

internal class SecretClientExtensionsTests(SecretClient secretClient)
    : IStravaUtilitiesTest
{
    internal async Task GetSecretThatExists()
    {
        var secret = await secretClient.GetSecretValue<string>($"StravaApiToken").ConfigureAwait(false);
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

    internal async Task AddTestSecret()
    {
        try
        {
            await secretClient.SetSecretAsync("TestSecretName-123", $"TestSecretValue-2025-01-01T00:00:00.000Z");
            ;
        }
        catch (Exception ex)
        {
            ;
        }
    }
}
