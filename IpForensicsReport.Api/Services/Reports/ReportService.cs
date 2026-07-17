using IpForensicsReport.Api.Exceptions;
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
                throw new InvalidIpAddressException(
                    locationData.Message
                    ?? "The IP address could not be processed.");
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

            return MapToResponse(reportId, payload);
        }

        public async Task<IReadOnlyList<IpForensicsReportResponse>> GetByUserIdAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }

            var encryptedReports =
                await _reportRepository.GetByUserIdAsync(
                    userId,
                    cancellationToken);

            var reports =
                new List<IpForensicsReportResponse>(
                    encryptedReports.Count);

            foreach (var encryptedReport in encryptedReports)
            {
                var encryptedData = new EncryptedReportData
                {
                    Payload =
                        encryptedReport.EncryptedPayload,

                    Nonce =
                        encryptedReport.EncryptionNonce,

                    AuthenticationTag =
                        encryptedReport.AuthenticationTag
                };

                var payloadJson =
                    _encryptionService.Decrypt(encryptedData);

                var payload =
                    JsonSerializer.Deserialize<ReportPayload>(
                        payloadJson);

                if (payload is null)
                {
                    throw new JsonException(
                        $"Report {encryptedReport.Id} " +
                        "could not be deserialized.");
                }

                reports.Add(
                    MapToResponse(
                        encryptedReport.Id,
                        payload));
            }

            return reports;
        }

        public async Task<IpForensicsReportResponse?> GetByIdAsync(
            long reportId,
            long userId,
            CancellationToken cancellationToken)
        {
            var encryptedReport =
                await _reportRepository.GetByIdAsync(
                    reportId,
                    userId,
                    cancellationToken);

            if (encryptedReport is null)
            {
                return null;
            }

            return DecryptAndMapReport(encryptedReport);
        }

        private IpForensicsReportResponse DecryptAndMapReport(
            EncryptedReportRecord encryptedReport)
        {
            var encryptedData = new EncryptedReportData
            {
                Payload = encryptedReport.EncryptedPayload,
                Nonce = encryptedReport.EncryptionNonce,
                AuthenticationTag =
                    encryptedReport.AuthenticationTag
            };

            var payloadJson =
                _encryptionService.Decrypt(encryptedData);

            var payload =
                JsonSerializer.Deserialize<ReportPayload>(
                    payloadJson)
                ?? throw new JsonException(
                    $"Report {encryptedReport.Id} contains invalid data.");

            payload.CreatedOn =
                encryptedReport.CreatedOnUtc;

            return MapToResponse(
                encryptedReport.Id,
                payload);
        }

        private static IpForensicsReportResponse MapToResponse(
            long reportId,
            ReportPayload payload)
        {
            return new IpForensicsReportResponse
            {
                Id = reportId,
                IpAddress = payload.IpAddress,
                AbuseConfidenceScore =
                    payload.AbuseConfidenceScore,
                TotalReports = payload.TotalReports,
                LastReportedDate =
                    payload.LastReportedDate,
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
                throw new InvalidIpAddressException(
                    "IP address is required.");
            }

            var trimmedIpAddress = ipAddress.Trim();

            if (!IPAddress.TryParse(
                    trimmedIpAddress,
                    out var parsedIpAddress))
            {
                throw new InvalidIpAddressException(
                    "The provided value is not a valid IPv4 or IPv6 address.");
            }

            return parsedIpAddress.ToString();
        }

        public async Task<int> DeleteAllByUserIdAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(
                    "A valid user ID is required.",
                    nameof(userId));
            }

            return await _reportRepository.DeleteByUserIdAsync(
                userId,
                cancellationToken);
        }
    }
}
