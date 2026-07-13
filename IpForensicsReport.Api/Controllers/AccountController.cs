using IpForensicsReport.Api.Models.Account;
using IpForensicsReport.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IpForensicsReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var response = await _accountService.RegisterAsync(request, cancellationToken);

            if (response is null)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Email already registered",
                    Detail = "An account with this email address already exists.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var response = await _accountService.LoginAsync(request, cancellationToken);

            if (response is null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed",
                    Detail = "Invalid email or password."
                });
            }

            return Ok(response);
        }
    }
}
