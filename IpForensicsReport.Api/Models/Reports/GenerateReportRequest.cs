using System.ComponentModel.DataAnnotations;

namespace IpForensicsReport.Api.Models.Reports
{
    public class GenerateReportRequest
    {
        public string IpAddress { get; set; } = string.Empty;
    }
}
