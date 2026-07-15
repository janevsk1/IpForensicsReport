using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<long> CreateAsync(CreateReportRecord report, CancellationToken cancellationToken);
    }
}
