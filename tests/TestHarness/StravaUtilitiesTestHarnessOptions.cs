namespace StravaUtilities.TestHarness;

public class StravaUtilitiesTestHarnessOptions
{
    public string? KeyVaultUri { get; set; }

    public long ActivityIdToGet { get; set; }
    public long ActivityIdToUpdate { get; set; }
    public string? ActivityToUploadFilePath { get; set; }

    public required long AthleteId { get; set; }

    public required string StravaApiClientId { get; set; }
    public required string StravaApiClientSecret { get; set; }
}
