using IpForensicsReport.Api.Configuration;
using IpForensicsReport.Api.Models.Reports;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace IpForensicsReport.Api.Services.Reports
{
    public class AbuseIpDbClient : IAbuseIpDbClient
    {
        private readonly HttpClient _httpClient;
        private readonly AbuseIpDbOptions _options;

        public AbuseIpDbClient(
            HttpClient httpClient,
            IOptions<AbuseIpDbOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<AbuseIpDbData> GetAsync(
            string ipAddress,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "AbuseIPDB API key is not configured.");
            }

            var encodedIpAddress = Uri.EscapeDataString(ipAddress);

            var requestUrl =
                $"api/v2/check" +
                $"?ipAddress={encodedIpAddress}" +
                $"&maxAgeInDays=90";

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                requestUrl);

            request.Headers.TryAddWithoutValidation(
                "Key",
                _options.ApiKey);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content
                    .ReadFromJsonAsync<AbuseIpDbResponse>(
                        cancellationToken: cancellationToken);

            if (result?.Data is null)
            {
                throw new HttpRequestException(
                    "AbuseIPDB returned an empty response.");
            }

            return result.Data;

            //try
            //{
            //    using var response = await _httpClient.SendAsync(
            //        request,
            //        cancellationToken);

            //    response.EnsureSuccessStatusCode();

            //    var result = await response.Content
            //        .ReadFromJsonAsync<AbuseIpDbResponse>(
            //            cancellationToken: cancellationToken);

            //    if (result?.Data is null)
            //    {
            //        throw new HttpRequestException(
            //            "AbuseIPDB returned an empty response.");
            //    }

            //    return result.Data;
            //}
            //catch (OperationCanceledException ex)
            //    when (!cancellationToken.IsCancellationRequested)
            //{
            //    throw new TimeoutException(
            //        $"AbuseIPDB did not respond within " +
            //        $"{_httpClient.Timeout.TotalSeconds} seconds.",
            //        ex);
            //}
            //catch (OperationCanceledException ex)
            //    when (cancellationToken.IsCancellationRequested)
            //{
            //    throw new OperationCanceledException(
            //        "The incoming report request was cancelled.",
            //        ex,
            //        cancellationToken);
            //}
        }
    }
}
