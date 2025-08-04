namespace StravaUtilities;
public class StravaUtilitiesException : ApplicationException
{
    internal StravaUtilitiesException(string message) : base(message) { }
    internal StravaUtilitiesException(string message, Exception innerException) : base(message, innerException) { }
}