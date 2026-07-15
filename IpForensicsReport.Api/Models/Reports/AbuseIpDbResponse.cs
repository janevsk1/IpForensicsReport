using System.Text.Json.Serialization;

namespace IpForensicsReport.Api.Models.Reports
{
    public class AbuseIpDbResponse
    {
        [JsonPropertyName("data")]
        public AbuseIpDbData? Data { get; set; }
    }

    public class AbuseIpDbData
    {
        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("abuseConfidenceScore")]
        public int AbuseConfidenceScore { get; set; }

        [JsonPropertyName("totalReports")]
        public int TotalReports { get; set; }

        [JsonPropertyName("lastReportedAt")]
        public DateTimeOffset? LastReportedAt { get; set; }

        [JsonPropertyName("isTor")]
        public bool IsTor { get; set; }
    }
}
