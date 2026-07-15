namespace IpForensicsReport.Api.Models.Reports
{
    public class EncryptedReportData
    {
        public byte[] Payload { get; set; } =
        Array.Empty<byte>();

        public byte[] Nonce { get; set; } =
            Array.Empty<byte>();

        public byte[] AuthenticationTag { get; set; } =
            Array.Empty<byte>();
    }
}
