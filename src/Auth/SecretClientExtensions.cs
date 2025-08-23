using Azure.Security.KeyVault.Secrets;
using System.Text.Json;

namespace StravaUtilities;

/// <summary>
/// Extensions for working with a <see cref="SecretClient"/>
/// </summary>
public static class SecretClientExtensions
{
    /// <summary>
    /// Gets the value of a secret and parses it to type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The expected type of the secret. If it is a complex type, the secret value should be serialized to json</typeparam>
    /// <param name="secretClient">The secret client to get from</param>
    /// <param name="secretKey">The key/name of the secret to get</param>
    /// <returns>The secret value as <typeparamref name="T"/>. Will either return an instance (not null), or an exception will be thrown.</returns>
    /// <exception cref="ApplicationException">If the secret was not found or the value was invalid</exception>
    /// <exception cref="ArgumentNullException">If either argument is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="secretKey"/> is an empty string</exception>
    public static async Task<T> GetSecretValue<T>(this SecretClient secretClient, string secretKey)
    {
        ArgumentNullException.ThrowIfNull(secretClient);
        ArgumentException.ThrowIfNullOrEmpty(secretKey);

        Azure.Response<KeyVaultSecret?> secretResponse;

        try
        {
            secretResponse = await secretClient.GetSecretAsync(secretKey).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Tried to get secret with key {secretKey} but an exception occurred when calling the vault", ex);
        }

        if (secretResponse == null)
            throw new ApplicationException($"Called vault to get secret with key {secretKey} but secret response was null");

        var keyVaultSecret = secretResponse.Value;
        if (keyVaultSecret == null)
            throw new ApplicationException($"Called vault to get secret with key {secretKey} but key vault secret was null");

        var secretValue = keyVaultSecret.Value;
        if (string.IsNullOrEmpty(secretValue))
            throw new ApplicationException($"Called vault to get secret with key {secretKey} but secret value was null or empty");

        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)secretValue;

            if (typeof(T).IsPrimitive)
            {
                var convertedValue = Convert.ChangeType(secretValue, typeof(T));
                if (convertedValue == null || convertedValue is not T)
                    throw new ApplicationException($"Value could not be converted to primitive type {typeof(T)}");

                return (T)convertedValue;
            }
            else
            {
                var parsedSecretValue = JsonSerializer.Deserialize<T>(secretValue);
                if (parsedSecretValue == null)
                    throw new ApplicationException($"Value was deserialized to type {typeof(T)} but the result was null");

                return parsedSecretValue;
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Got secret value with key {secretKey}, but it could not be parsed to type {typeof(T)}", ex);
        }
    }
}
