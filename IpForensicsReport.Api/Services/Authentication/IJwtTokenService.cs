using IpForensicsReport.Api.Models.User;

namespace IpForensicsReport.Api.Services.Authentication
{
    public interface IJwtTokenService
    {
        JwtTokenResult CreateToken(User user);
    }
}
