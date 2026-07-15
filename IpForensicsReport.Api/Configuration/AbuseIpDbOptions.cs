namespace IpForensicsReport.Api.Configuration
{
    public class AbuseIpDbOptions
    {
        public const string SectionName = "ExternalApis:AbuseIpDb";

        public string BaseUrl { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;
    }
}
