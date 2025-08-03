using StravaUtilities.Models;

namespace StravaUtilities.ApiClient;

public partial class StravaApiClient
{
    public Athlete CurrentAuthenticatedAthlete { get; private set; }

    private async Task SetCurrentAthlete()
    {
        var athlete = await _httpClient.Get<Athlete>("athlete").ConfigureAwait(false);
        CurrentAuthenticatedAthlete = athlete;
    }
}