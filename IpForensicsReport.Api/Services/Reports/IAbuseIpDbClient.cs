using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Services.Reports
{
    public interface IAbuseIpDbClient
    {
        Task<AbuseIpDbData> GetAsync(string ipAddress, CancellationToken cancellationToken);
    }
}
