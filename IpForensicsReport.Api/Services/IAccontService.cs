using IpForensicsReport.Api.Models.Account;

namespace IpForensicsReport.Api.Services;

public interface IAccountService
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
