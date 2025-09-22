using System.Text.Json.Serialization;

namespace StravaUtilities;

public class PolylineMap
{
    [JsonPropertyName("summary_polyline")]
    public string SummaryPolyline { get; set; }

}