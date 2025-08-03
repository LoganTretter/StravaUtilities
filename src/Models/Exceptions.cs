namespace StravaUtilities.Models;

public class StravaUtilitiesException : ApplicationException
{
    public StravaUtilitiesException(string message) : base(message) { }
    public StravaUtilitiesException(string message, Exception innerException) : base(message, innerException) { }
}