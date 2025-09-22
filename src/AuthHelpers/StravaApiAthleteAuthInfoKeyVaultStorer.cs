using Azure.Security.KeyVault.Secrets;
using System.Text.Json;

namespace StravaUtilities;

/// <summary>
/// Provides methods for storing strava athlete auth information in an Azure Key Vault
/// </summary>
/// <param name="secretClient"></param>
/// <inheritdoc />
public class StravaApiAthleteAuthInfoKeyVaultStorer(SecretClient secretClient) : IStravaApiAthleteAuthInfoStorer
{
    public const string KeyVaultSecretNameTemplate = "StravaApiAthleteAuthInfo-{0}";

    public async Task<StravaApiAthleteAuthInfo> GetAthleteAuthInfo(long athleteId)
    {
        StravaApiAthleteAuthInfo? authInfoFromVault;

        try
        {
            authInfoFromVault = await secretClient.GetSecretValue<StravaApiAthleteAuthInfo>(string.Format(KeyVaultSecretNameTemplate, athleteId)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var msg = "Tried to get auth info from key vault storage but an exception occurred";
            throw new StravaUtilitiesException(msg, innerException: ex);
        }

        if (authInfoFromVault == null)
            throw new StravaUtilitiesException($"Called key vault storage to get auth info, but {nameof(authInfoFromVault)} was null");
        if (authInfoFromVault.TokenInfo == null)
            throw new StravaUtilitiesException($"Got auth info from key vault storage, but {nameof(StravaApiAthleteAuthInfo.TokenInfo)} was null");
        if (string.IsNullOrEmpty(authInfoFromVault.TokenInfo.AccessToken))
            throw new StravaUtilitiesException($"Got auth info from key vault storage, but {nameof(StravaApiTokenInfo.AccessToken)} was null or empty");
        if (string.IsNullOrEmpty(authInfoFromVault.TokenInfo.RefreshToken))
            throw new StravaUtilitiesException($"Got auth info from key vault storage, but {nameof(StravaApiTokenInfo.RefreshToken)} was null or empty");

        return authInfoFromVault;
    }

    public async Task AddOrUpdateAthleteAuthInfo(StravaApiAthleteAuthInfo authInfo)
    {
        var name = string.Format(KeyVaultSecretNameTemplate, authInfo.AthleteId);
        var value = JsonSerializer.Serialize(authInfo);

        await secretClient.SetSecretAsync(name, value).ConfigureAwait(false);
    }
}
