namespace StravaUtilities.Models;

public class LatitudeLongitude
{
    public LatitudeLongitude(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}