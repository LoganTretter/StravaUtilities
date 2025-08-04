using System.Text.Json.Serialization;

namespace StravaUtilities.ApiClient;

internal class StravaApiErrorResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    [JsonPropertyName("errors")]
    public List<StravaApiErrorDetail> Errors { get; set; } = [];

    internal class StravaApiErrorDetail
    {
        [JsonPropertyName("resource")]
        public string Resource { get; set; } = "";
        [JsonPropertyName("field")]
        public string Field { get; set; } = "";
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";
    }
}
