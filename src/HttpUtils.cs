using Polly;
using Polly.Extensions.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Http;

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
            throw new StravaApiException(ex.Message, ex);
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
            throw new StravaApiException(ex.Message, ex);
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
            throw new StravaApiException(ex.Message, ex);
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
            throw new StravaApiException(ex.Message, ex);
        }
    }

    internal static async Task<T> ParseResponse<T>(HttpResponseMessage response, string pathUsed)
    {
        await EnsureResponseSuccess(response, pathUsed).ConfigureAwait(false);

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseString))
            throw new StravaApiException($"Problem reading Strava API call response for path: '{pathUsed}'. Status is {(int)response.StatusCode} {response.StatusCode} but response content was empty.");

        try
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            var item = JsonSerializer.Deserialize<T>(responseString, options);
            return item;
        }
        catch (Exception ex)
        {
            throw new StravaApiException($"Problem deserializing Strava API call response for path: '{pathUsed}' - {ex.Message}", ex);
        }
    }

    internal static async Task EnsureResponseSuccess(HttpResponseMessage response, string pathUsed)
    {
        if (response.IsSuccessStatusCode)
            return;
        
        var error = await response.Content.ReadAsAsync<HttpError>().ConfigureAwait(false);
        string message = $"{(int)response.StatusCode} {response.StatusCode} error in Strava API call for path: '{pathUsed}'";
        if (!string.IsNullOrEmpty(error.Message))
            message += $"{Environment.NewLine}{nameof(HttpError.Message)}: {error.Message}";
        if (!string.IsNullOrEmpty(error.MessageDetail))
            message += $"{Environment.NewLine}{nameof(HttpError.MessageDetail)}: {error.MessageDetail}";
        if (!string.IsNullOrEmpty(error.ExceptionMessage))
            message += $"{Environment.NewLine}{nameof(HttpError.ExceptionMessage)}: {error.ExceptionMessage}";

        throw new StravaApiException(message);
    }
}