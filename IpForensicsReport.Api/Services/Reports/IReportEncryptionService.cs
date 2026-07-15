using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Services.Reports
{
    public interface IReportEncryptionService
    {
        EncryptedReportData Encrypt(string plaintext);

        string Decrypt(EncryptedReportData encryptedData);
    }
}
