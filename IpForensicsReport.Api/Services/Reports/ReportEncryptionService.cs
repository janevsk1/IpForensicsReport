using IpForensicsReport.Api.Configuration;
using IpForensicsReport.Api.Models.Reports;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace IpForensicsReport.Api.Services.Reports
{
    public class ReportEncryptionService : IReportEncryptionService
    {
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly byte[] _key;

        public ReportEncryptionService(
            IOptions<ReportEncryptionOptions> options)
        {
            var configuredKey = options.Value.Key;

            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                throw new InvalidOperationException(
                    "Report encryption key is not configured.");
            }

            try
            {
                _key = Convert.FromBase64String(configuredKey);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException(
                    "Report encryption key must be Base64 encoded.",
                    exception);
            }

            if (_key.Length != KeySize)
            {
                throw new InvalidOperationException(
                    "Report encryption key must contain 32 bytes.");
            }
        }

        public EncryptedReportData Encrypt(string plaintext)
        {
            ArgumentNullException.ThrowIfNull(plaintext);

            var plaintextBytes =
                Encoding.UTF8.GetBytes(plaintext);

            var encryptedBytes =
                new byte[plaintextBytes.Length];

            var nonce = new byte[NonceSize];
            var authenticationTag = new byte[TagSize];

            RandomNumberGenerator.Fill(nonce);

            using var aes = new AesGcm(
                _key,
                TagSize);

            aes.Encrypt(
                nonce,
                plaintextBytes,
                encryptedBytes,
                authenticationTag);

            return new EncryptedReportData
            {
                Payload = encryptedBytes,
                Nonce = nonce,
                AuthenticationTag = authenticationTag
            };
        }

        public string Decrypt(
            EncryptedReportData encryptedData)
        {
            ArgumentNullException.ThrowIfNull(encryptedData);

            var plaintextBytes =
                new byte[encryptedData.Payload.Length];

            using var aes = new AesGcm(
                _key,
                TagSize);

            aes.Decrypt(
                encryptedData.Nonce,
                encryptedData.Payload,
                encryptedData.AuthenticationTag,
                plaintextBytes);

            return Encoding.UTF8.GetString(plaintextBytes);
        }
    }
}
