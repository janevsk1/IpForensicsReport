using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Services.Reports
{
    public interface IReportService
    {
        Task<IpForensicsReportResponse> GenerateAsync(
            long userId,
            string ipAddress,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<IpForensicsReportResponse>> GetByUserIdAsync(
            long userId,
            CancellationToken cancellationToken);

        Task<IpForensicsReportResponse?> GetByIdAsync(
            long reportId,
            long userId,
            CancellationToken cancellationToken);

        Task<int> DeleteAllByUserIdAsync(
            long userId,
            CancellationToken cancellationToken);
    }
}
