using Azure.Security.KeyVault.Secrets;
using System.Text.Json;

namespace StravaUtilities;

/// <summary>
/// Provides methods for storing strava token information in an Azure Key Vault
/// </summary>
/// <param name="secretClient"></param>
/// <inheritdoc />
public class StravaApiTokenKeyVaultStorer(SecretClient secretClient) : IStravaApiTokenStorer
{
    public async Task<StravaApiToken> GetToken()
    {
        StravaApiToken? tokenFromVault;

        try
        {
            tokenFromVault = await secretClient.GetSecretValue<StravaApiToken>("StravaApiToken").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Tried to get token from key vault storage but an exception occurred", ex);
        }

        if (tokenFromVault == null)
            throw new ApplicationException($"Called key vault storage to get token, but {nameof(tokenFromVault)} was null.");
        if (string.IsNullOrEmpty(tokenFromVault.AccessToken))
            throw new ApplicationException($"Got token from key vault storage, but {nameof(tokenFromVault)}.{nameof(StravaApiToken.AccessToken)} was null or empty.");
        if (string.IsNullOrEmpty(tokenFromVault.RefreshToken))
            throw new ApplicationException($"Got token from key vault storage, but {nameof(tokenFromVault)}.{nameof(StravaApiToken.RefreshToken)} was null or empty.");

        return tokenFromVault;
    }

    public async Task AddOrUpdateToken(StravaApiToken token)
    {
        await secretClient.SetSecretAsync("StravaApiToken", JsonSerializer.Serialize(token)).ConfigureAwait(false);
    }
}
