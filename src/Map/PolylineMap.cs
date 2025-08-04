using System.Text.Json.Serialization;

namespace StravaApi;

public class PolylineMap
{
    [JsonPropertyName("summary_polyline")]
    public string SummaryPolyline { get; set; }

}