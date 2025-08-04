using Polly;
using Polly.Extensions.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StravaUtilities;

internal static class HttpUtils
{
    // TODO timeout, cancellation token
    private static readonly IAsyncPolicy<HttpResponseMessage> TransientRetryPolicy = HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(800) });

    internal static async Task<T> Get<T>(this HttpClient httpClient, string relativePath)
    {
        try
        {
            using (var response = await TransientRetryPolicy.ExecuteAsync(() => httpClient.GetAsync(new Uri(relativePath, UriKind.Relative))).ConfigureAwait(false))
            {
                var result = await ParseResponse<T>(response, relativePath).ConfigureAwait(false);
                return result;
            }
        }
        catch (HttpRequestException ex)
        {
            throw new StravaUtilitiesException(ex.Message, ex);
        }
    }

    internal static async Task<T> Post<T>(this HttpClient httpClient, string relativePath, HttpContent content = null)
    {
        try
        {
            using (var response = await httpClient.PostAsync(new Uri(relativePath, UriKind.Relative), content).ConfigureAwait(false))
            {
                var result = await ParseResponse<T>(response, relativePath).ConfigureAwait(false);
                return result;
            }
        }
        catch (HttpRequestException ex)
        {
            // TODO could it get the full request uri into this message?
            throw new StravaUtilitiesException(ex.Message, ex);
        }
    }

    internal static async Task<T> Put<T>(this HttpClient httpClient, string relativePath, HttpContent content = null)
    {
        try
        {
            using (var response = await httpClient.PutAsync(new Uri(relativePath, UriKind.Relative), content).ConfigureAwait(false))
            {
                var result = await ParseResponse<T>(response, relativePath).ConfigureAwait(false);
                return result;
            }
        }
        catch (HttpRequestException ex)
        {
            throw new StravaUtilitiesException(ex.Message, ex);
        }
    }

    internal static async Task Delete(this HttpClient httpClient, string relativePath)
    {
        try
        {
            using (var response = await httpClient.DeleteAsync(new Uri(relativePath, UriKind.Relative)).ConfigureAwait(false))
            {
                await EnsureResponseSuccess(response, relativePath).ConfigureAwait(false);
            }
        }
        catch (HttpRequestException ex)
        {
            throw new StravaUtilitiesException(ex.Message, ex);
        }
    }

    internal static async Task<T> ParseResponse<T>(HttpResponseMessage response, string pathUsed)
    {
        await EnsureResponseSuccess(response, pathUsed).ConfigureAwait(false);

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseString))
            throw new StravaUtilitiesException($"Problem reading Strava API call response for path: '{pathUsed}'. Status is {(int)response.StatusCode} {response.StatusCode} but response content was empty.");

        try
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            var item = JsonSerializer.Deserialize<T>(responseString, options);
            return item;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException($"Problem deserializing Strava API call response for path: '{pathUsed}' - {ex.Message}", ex);
        }
    }

    internal static async Task EnsureResponseSuccess(HttpResponseMessage response, string pathUsed)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message = $"{(int)response.StatusCode} {response.StatusCode} error in Strava API call for path: '{pathUsed}'";
        
        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
            message += $"{Environment.NewLine}Response did not specify additional info.";
        else
            message += $"{Environment.NewLine}Response content: {responseContent}";

        throw new StravaUtilitiesException(message);
    }
}