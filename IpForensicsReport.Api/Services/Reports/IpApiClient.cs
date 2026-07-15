using IpForensicsReport.Api.Models.Reports;

namespace IpForensicsReport.Api.Services.Reports
{
    public class IpApiClient : IIpApiClient
    {
        private readonly HttpClient _httpClient;

        public IpApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IpApiResponse> GetAsync(
            string ipAddress,
            CancellationToken cancellationToken)
        {
            var encodedIpAddress = Uri.EscapeDataString(ipAddress);

            var fields = string.Join(
                ",",
                "status",
                "message",
                "query",
                "continent",
                "country",
                "regionName",
                "city",
                "mobile",
                "proxy",
                "hosting");

            var requestUrl =
                $"json/{encodedIpAddress}?fields={fields}";

            using var response = await _httpClient.GetAsync(
                requestUrl,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<IpApiResponse>(
                    cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new HttpRequestException(
                    "ip-api returned an empty response.");
            }

            return result;
        }
    }
}
