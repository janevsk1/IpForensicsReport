using IpForensicsReport.Api.Models.Reports;
using IpForensicsReport.Api.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IpForensicsReport.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReportController(
        IReportService reportService) : ControllerBase
    {
        private readonly IReportService _reportService = reportService;

        [HttpPost("generate")]
        public async Task<ActionResult<IpForensicsReportResponse>> GenerateReport(
            [FromBody] GenerateReportRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized(GetInvalidAccessTokenProblemDetails());
            }

            var report = await _reportService.GenerateAsync(
                userId,
                request.IpAddress,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                report);
        }

        [HttpGet("reports")]
        public async Task<ActionResult<IReadOnlyList<IpForensicsReportResponse>>> GetReports(CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized(GetInvalidAccessTokenProblemDetails());
            }

            var reports = await _reportService.GetByUserIdAsync(
                userId,
                cancellationToken);

            return Ok(reports);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<IpForensicsReportResponse>> GetById(
            long id,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized(GetInvalidAccessTokenProblemDetails());
            }

            var report = await _reportService.GetByIdAsync(
                id,
                userId,
                cancellationToken);

            if (report is null)
            {
                return NotFound(new
                {
                    Message = "Report was not found."
                });
            }

            return Ok(report);
        }

        private bool TryGetAuthenticatedUserId(out long userId)
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            return long.TryParse(userIdClaim, out userId);
        }

        private static ProblemDetails GetInvalidAccessTokenProblemDetails()
        {
            return new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid access token",
                Detail = "The access token does not contain a valid user ID."
            };
        }
    }
}
