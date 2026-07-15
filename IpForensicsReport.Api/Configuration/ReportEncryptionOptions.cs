namespace IpForensicsReport.Api.Configuration
{
    public class ReportEncryptionOptions
    {
        public const string SectionName = "ReportEncryption";

        public string Key { get; set; } = string.Empty;
    }
}
