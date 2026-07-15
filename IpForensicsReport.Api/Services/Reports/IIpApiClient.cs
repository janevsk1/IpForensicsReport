using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Services.Reports
{
    public interface IIpApiClient
    {
        Task<IpApiResponse> GetAsync(string ipAddress, CancellationToken cancellationToken);
    }
}
