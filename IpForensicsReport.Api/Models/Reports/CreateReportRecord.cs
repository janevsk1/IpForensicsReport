namespace IpForensicsReport.Api.Models.Reports
{
    public class CreateReportRecord
    {
        public long UserId { get; set; }

        public byte[] EncryptedPayload { get; set; } =
            Array.Empty<byte>();

        public byte[] EncryptionNonce { get; set; } =
            Array.Empty<byte>();

        public byte[] AuthenticationTag { get; set; } =
            Array.Empty<byte>();

        public DateTime CreatedOnUtc { get; set; }
    }
}
