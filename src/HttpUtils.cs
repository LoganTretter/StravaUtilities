using Polly;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StravaUtilities;

internal static class HttpUtils
{
    private static readonly ResiliencePipeline<HttpResponseMessage> ResiliencePipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 4, // retries after initial call
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(500), // median delay
                MaxDelay = TimeSpan.FromSeconds(2),
                ShouldHandle = args =>
                {
                    var outcome = args.Outcome;

                    // If there was an exception, retry unless it was a cancellation
                    if (outcome.Exception != null)
                    {
                        if (outcome.Exception is OperationCanceledException)
                            return new ValueTask<bool>(false);

                        return new ValueTask<bool>(true);
                    }

                    var response = outcome.Result;
                    if (response == null)
                        return new ValueTask<bool>(false);

                    if (response.StatusCode == System.Net.HttpStatusCode.RequestTimeout) // 408
                        return new ValueTask<bool>(true);

                    if ((int)response.StatusCode >= 500) // 5xx server errors
                        return new ValueTask<bool>(true);

                    return new ValueTask<bool>(false);
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(10)) // Add 10 seconds timeout
            .Build();

    internal static async Task<T> Get<T>(this HttpClient httpClient, string relativePath, StravaApiAthleteAuthInfo? authInfo = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        return await httpClient.SendRequestAndParseResponse<T>(HttpMethod.Get, relativePath, authInfo, content, mediaTypeName).ConfigureAwait(false);
    }

    internal static async Task<T> Post<T>(this HttpClient httpClient, string relativePath, StravaApiAthleteAuthInfo? authInfo = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        return await httpClient.SendRequestAndParseResponse<T>(HttpMethod.Post, relativePath, authInfo, content, mediaTypeName).ConfigureAwait(false);
    }

    internal static async Task<T> Put<T>(this HttpClient httpClient, string relativePath, StravaApiAthleteAuthInfo? authInfo = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        return await httpClient.SendRequestAndParseResponse<T>(HttpMethod.Put, relativePath, authInfo, content, mediaTypeName).ConfigureAwait(false);
    }

    internal static async Task Delete(this HttpClient httpClient, string relativePath, StravaApiAthleteAuthInfo? authInfo = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        try
        {
            using var request = GetHttpRequestMessage(HttpMethod.Delete, relativePath, authInfo, content, mediaTypeName);

            using var response = await httpClient.DeleteAsync(new Uri(relativePath, UriKind.Relative)).ConfigureAwait(false);

            await EnsureResponseSuccess(response, relativePath).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new StravaUtilitiesException(ex.Message, ex);
        }
    }

    private static HttpRequestMessage GetHttpRequestMessage(HttpMethod method, string relativePath, StravaApiAthleteAuthInfo? authInfo = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        var request = new HttpRequestMessage(method, new Uri(relativePath, UriKind.Relative));

        if (authInfo?.TokenInfo != null && !string.IsNullOrWhiteSpace(authInfo.TokenInfo.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authInfo.TokenInfo.AccessToken);

        if (!string.IsNullOrEmpty(mediaTypeName))
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaTypeName));

        if (content != null)
            request.Content = content;

        return request;
    }

    private static async Task<T> SendRequestAndParseResponse<T>(this HttpClient httpClient, HttpMethod method, string relativePath, StravaApiAthleteAuthInfo? token = null, HttpContent? content = null, string? mediaTypeName = null)
    {
        var response = await ResiliencePipeline.ExecuteAsync(async cancellationToken =>
        {
            using var request = GetHttpRequestMessage(method, relativePath, token, content, mediaTypeName);
            return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        });

        var result = await ParseResponse<T>(response, relativePath).ConfigureAwait(false);
        return result;
    }

    internal static async Task<T> ParseResponse<T>(HttpResponseMessage response, string pathUsed)
    {
        await EnsureResponseSuccess(response, pathUsed).ConfigureAwait(false);

        var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseString))
            throw new StravaUtilitiesException($"Problem reading Strava API call response for path '{pathUsed}': status is {(int)response.StatusCode} {response.StatusCode} but response content was empty");

        T? item;
        try
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());

            item = JsonSerializer.Deserialize<T>(responseString, options);
        }
        catch (Exception ex)
        {
            var msg = $"Problem deserializing Strava API call response for path '{pathUsed}': {ex.Message}";
            throw new StravaUtilitiesException(msg, innerException: ex);
        }

        if (item == null)
            throw new StravaUtilitiesException($"Problem parsing Strava API call response for path '{pathUsed}': deserialization succeeded but value was null");

        return item;
    }

    private static async Task EnsureResponseSuccess(HttpResponseMessage response, string pathUsed)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message = $"{(int)response.StatusCode} {response.StatusCode} error in Strava API call for path '{pathUsed}'";

        var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(responseContent))
            message += $"{Environment.NewLine}Response did not specify additional info.";
        else
            message += $"{Environment.NewLine}Response content: {responseContent}";

        throw new StravaUtilitiesException(message);
    }
}