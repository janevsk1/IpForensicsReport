using System.Text.Json.Serialization;

namespace IpForensicsReport.Api.Models.Reports
{
    public class IpApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("query")]
        public string Query { get; set; } = string.Empty;

        [JsonPropertyName("continent")]
        public string? Continent { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("regionName")]
        public string? RegionName { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("mobile")]
        public bool Mobile { get; set; }

        [JsonPropertyName("proxy")]
        public bool Proxy { get; set; }

        [JsonPropertyName("hosting")]
        public bool Hosting { get; set; }
    }
}
