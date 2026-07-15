using IpForensicsReport.Api.Models.Reports;
using IpForensicsReport.Api.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace IpForensicsReport.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<IpForensicsReportResponse>> GenerateReport(
            [FromBody] GenerateReportRequest request,
            CancellationToken cancellationToken)
        {
            //var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
            //    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message = "The access token does not contain a valid user ID."
                });
            }

            try
            {
                var report = await _reportService.GenerateAsync(
                        userId,
                        request.IpAddress,
                        cancellationToken);

                return StatusCode(
                    StatusCodes.Status201Created,
                    report);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    Message = exception.Message
                });
            }
            catch (HttpRequestException exception)
            {
                _logger.LogWarning(
                    exception,
                    "External API request failed while generating report.");

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        Message = "An external IP information service is unavailable."
                    });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<IpForensicsReportResponse>>> GetReports(CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized(new
                {
                    Message =
                        "The access token does not contain " +
                        "a valid user ID."
                });
            }

            try
            {
                var reports =
                    await _reportService.GetByUserIdAsync(
                        userId,
                        cancellationToken);

                return Ok(reports);
            }
            catch (CryptographicException exception)
            {
                _logger.LogError(
                    exception,
                    "A saved report could not be decrypted " +
                    "for user {UserId}.",
                    userId);

                return Problem(
                    statusCode:
                        StatusCodes.Status500InternalServerError,
                    title:
                        "A saved report could not be decrypted.");
            }
            catch (JsonException exception)
            {
                _logger.LogError(
                    exception,
                    "A decrypted report contained invalid data " +
                    "for user {UserId}.",
                    userId);

                return Problem(
                    statusCode:
                        StatusCodes.Status500InternalServerError,
                    title:
                        "A saved report contained invalid data.");
            }
        }

        private bool TryGetAuthenticatedUserId(out long userId)
        {
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            return long.TryParse(userIdClaim, out userId);
        }
    }
}
