namespace IpForensicsReport.Api.Exceptions
{
    public class InvalidIpAddressException : Exception
    {
        public InvalidIpAddressException(string message) : base(message)
        {
        }
    }
}
