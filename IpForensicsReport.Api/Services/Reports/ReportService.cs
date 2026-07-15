using IpForensicsReport.Api.Models.Reports;
using IpForensicsReport.Api.Repositories.Interfaces;
using System.Net;
using System.Text.Json;

namespace IpForensicsReport.Api.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IIpApiClient _ipApiClient;
        private readonly IAbuseIpDbClient _abuseIpDbClient;
        private readonly IReportEncryptionService
            _encryptionService;
        private readonly IReportRepository _reportRepository;

        public ReportService(
            IIpApiClient ipApiClient,
            IAbuseIpDbClient abuseIpDbClient,
            IReportEncryptionService encryptionService,
            IReportRepository reportRepository)
        {
            _ipApiClient = ipApiClient;
            _abuseIpDbClient = abuseIpDbClient;
            _encryptionService = encryptionService;
            _reportRepository = reportRepository;
        }

        public async Task<IpForensicsReportResponse> GenerateAsync(
            long userId,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }

            var normalizedIpAddress = ValidateAndNormalizeIpAddress(ipAddress);

            /*
             * Call ip-api first because it returns a clear failure
             * response for invalid, private, or reserved addresses.
             */
            var locationData =
                await _ipApiClient.GetAsync(
                    normalizedIpAddress,
                    cancellationToken);

            if (!string.Equals(
                    locationData.Status,
                    "success",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    locationData.Message ??
                    "The IP address could not be processed.",
                    nameof(ipAddress));
            }

            var abuseData =
                await _abuseIpDbClient.GetAsync(
                    normalizedIpAddress,
                    cancellationToken);

            var createdOnUtc = DateTime.UtcNow;

            var payload = new ReportPayload
            {
                IpAddress = normalizedIpAddress,
                AbuseConfidenceScore = abuseData.AbuseConfidenceScore,
                TotalReports = abuseData.TotalReports,
                LastReportedDate = abuseData.LastReportedAt,
                Continent = locationData.Continent,
                Country = locationData.Country,
                Region = locationData.RegionName,
                City = locationData.City,
                Mobile = locationData.Mobile,
                Proxy = locationData.Proxy,
                Hosting = locationData.Hosting,
                Tor = abuseData.IsTor,
                CreatedOn = createdOnUtc
            };

            var payloadJson = JsonSerializer.Serialize(payload);

            var encryptedData = _encryptionService.Encrypt(payloadJson);

            var reportId = await _reportRepository.CreateAsync(
                    new CreateReportRecord
                    {
                        UserId = userId,
                        EncryptedPayload = encryptedData.Payload,
                        EncryptionNonce = encryptedData.Nonce,
                        AuthenticationTag = encryptedData.AuthenticationTag,
                        CreatedOnUtc = createdOnUtc
                    },
                    cancellationToken);

            return new IpForensicsReportResponse
            {
                Id = reportId,
                IpAddress = payload.IpAddress,
                AbuseConfidenceScore = payload.AbuseConfidenceScore,
                TotalReports = payload.TotalReports,
                LastReportedDate = payload.LastReportedDate,
                Continent = payload.Continent,
                Country = payload.Country,
                Region = payload.Region,
                City = payload.City,
                Mobile = payload.Mobile,
                Proxy = payload.Proxy,
                Hosting = payload.Hosting,
                Tor = payload.Tor,
                CreatedOn = payload.CreatedOn
            };
        }

        private static string ValidateAndNormalizeIpAddress(
            string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                throw new ArgumentException(
                    "IP address is required.",
                    nameof(ipAddress));
            }

            var trimmedIpAddress = ipAddress.Trim();

            if (!IPAddress.TryParse(
                    trimmedIpAddress,
                    out var parsedIpAddress))
            {
                throw new ArgumentException(
                    "The provided value is not a valid IPv4 or IPv6 address.",
                    nameof(ipAddress));
            }

            return parsedIpAddress.ToString();
        }
    }
}
