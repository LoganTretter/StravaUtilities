namespace StravaApi;
public class StravaApiException : ApplicationException
{
    internal StravaApiException(string message) : base(message) { }
    internal StravaApiException(string message, Exception innerException) : base(message, innerException) { }
}