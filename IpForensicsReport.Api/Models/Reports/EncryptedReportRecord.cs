namespace IpForensicsReport.Api.Models.Reports
{
    public class EncryptedReportRecord
    {
        public long Id { get; set; }

        public byte[] EncryptedPayload { get; set; } = 
            Array.Empty<byte>();

        public byte[] EncryptionNonce { get; set; } =
            Array.Empty<byte>();

        public byte[] AuthenticationTag { get; set; } =
            Array.Empty<byte>();

        public DateTime CreatedOnUtc { get; set; }
    }
}
