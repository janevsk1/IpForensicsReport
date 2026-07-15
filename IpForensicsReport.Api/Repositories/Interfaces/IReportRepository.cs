using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<long> CreateAsync(CreateReportRecord report, CancellationToken cancellationToken);

        Task<IReadOnlyList<EncryptedReportRecord>> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

        Task<EncryptedReportRecord?> GetByIdAsync(long reportId, long userId, CancellationToken cancellationToken);
    }
}
