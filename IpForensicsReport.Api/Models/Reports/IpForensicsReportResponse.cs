namespace IpForensicsReport.Api.Models.Reports
{
    public class IpForensicsReportResponse
    {
        public long Id { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public int AbuseConfidenceScore { get; set; }

        public int TotalReports { get; set; }

        public DateTimeOffset? LastReportedDate { get; set; }

        public string? Continent { get; set; }

        public string? Country { get; set; }

        public string? Region { get; set; }

        public string? City { get; set; }

        public bool Mobile { get; set; }

        public bool Proxy { get; set; }

        public bool Hosting { get; set; }

        public bool Tor { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
