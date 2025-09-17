using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace StravaUtilities;

/// <summary>
/// A class that allows 
/// </summary>
internal class LocalServer : IDisposable
{
    private readonly HttpListener _listener;
    private bool _started;

    internal LocalServer(string listenerUriPrefix)
    {
        if (string.IsNullOrEmpty(listenerUriPrefix))
            throw new ArgumentNullException(nameof(listenerUriPrefix));

        _listener = new HttpListener();
        _listener.Prefixes.Add(listenerUriPrefix);
    }

    internal static void OpenWebPage(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Waits for a single incoming request and returns the query parameters from the request.
    /// </summary>
    /// <param name="timeout">Optional timeout; if null, default is 1 minute</param>
    /// <returns>Name/value pairs from the query of the http request</returns>
    internal async Task<Dictionary<string, string>> WaitForRequestAndGetQueryParams(TimeSpan? timeout = null)
    {
        if (!_started)
        {
            _listener.Start();
            _started = true;
        }

        var getContextTask = _listener.GetContextAsync();
        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(1);

        var completed = await Task.WhenAny(getContextTask, Task.Delay(effectiveTimeout)).ConfigureAwait(false);
        if (completed != getContextTask)
        {
            StopListener();
            throw new TimeoutException($"No HTTP request received within {effectiveTimeout.TotalSeconds} seconds.");
        }

        var context = await getContextTask.ConfigureAwait(false);

        try
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string? key in context.Request.QueryString)
            {
                if (key is null)
                    continue;

                var value = context.Request.QueryString[key];
                if (value != null)
                    result[key] = value;
            }

            // Simple response to inform the user the auth completed.
            const string responseHtml = @"
<html>
  <body>
    <h1>Authorization complete</h1>
    <p>You can now close this window.</p>
  </body>
</html>";
            
            var buffer = Encoding.UTF8.GetBytes(responseHtml);
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

            context.Response.OutputStream.Close();

            return result;
        }
        finally
        {
            StopListener();
        }
    }

    private void StopListener()
    {
        if (_listener.IsListening)
        {
            try
            {
                _listener.Stop();
            }
            catch { }
        }
        _started = false;
    }

    public void Dispose()
    {
        StopListener();
        try
        {
            _listener.Close();
        }
        catch { }
    }
}