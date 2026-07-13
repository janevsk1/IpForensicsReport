using IpForensicsReport.Api.Models.Account;
using IpForensicsReport.Api.Models.User;
using IpForensicsReport.Api.Repositories.Interfaces;
using IpForensicsReport.Api.Services.Authentication;
using Microsoft.AspNetCore.Identity;

namespace IpForensicsReport.Api.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AccountService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            CreatedOn = DateTime.UtcNow
        };

        user.Password = _passwordHasher.HashPassword(user, request.Password);

        var userId = await _userRepository.TryCreateAsync(user, cancellationToken);

        if (userId is null)
        {
            return null;
        }

        return new RegisterResponse
        {
            Id = userId.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail =  request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var tokenResult = _jwtTokenService.CreateToken(user);

        return new LoginResponse
        {
            AccessToken = tokenResult.AccessToken,
            TokenType = "Bearer",
            ExpiresAtUtc = tokenResult.ExpiresAtUtc,
            User = new AuthenticatedUserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }
}