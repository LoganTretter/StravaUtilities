namespace StravaUtilities;

public partial class StravaApiClient
{
    public Athlete? CurrentAuthenticatedAthlete { get; private set; }

    private async Task<Athlete> SetCurrentAthlete()
    {
        try
        {
            var athlete = await HttpClient.Get<Athlete>("athlete").ConfigureAwait(false);

            if (athlete == null)
                throw new StravaUtilitiesException("Could not get current authenticated athlete.");

            CurrentAuthenticatedAthlete = athlete;
            return athlete;
        }
        catch (Exception ex)
        {
            throw new StravaUtilitiesException("Could not get current authenticated athlete.", ex);
        }
    }
}