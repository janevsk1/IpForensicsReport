namespace IpForensicsReport.Api.Configuration
{
    public class IpApiOptions
    {
        public const string SectionName = "ExternalApis:IpApi";

        public string BaseUrl { get; set; } = string.Empty;
    }
}
