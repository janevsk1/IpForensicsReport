using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IpForensicsReport.Api.Configuration;
using IpForensicsReport.Api.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IpForensicsReport.Api.Services.Authentication
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public JwtTokenResult CreateToken(User user)
        {
            if (string.IsNullOrWhiteSpace(_options.SigningKey))
            {
                throw new InvalidOperationException(
                    "JWT signing key is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.Issuer))
            {
                throw new InvalidOperationException(
                    "JWT issuer is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.Audience))
            {
                throw new InvalidOperationException(
                    "JWT audience is not configured.");
            }

            var now = DateTime.UtcNow;

            var expiration = now.AddMinutes(
                _options.ExpirationMinutes);

            var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString(CultureInfo.InvariantCulture)),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.GivenName,
                user.FirstName),

            new(
                ClaimTypes.Surname,
                user.LastName),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SigningKey));

            var signingCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: expiration,
                signingCredentials: signingCredentials);

            var serializedToken =
                new JwtSecurityTokenHandler().WriteToken(token);

            return new JwtTokenResult
            {
                AccessToken = serializedToken,
                ExpiresAtUtc = expiration
            };
        }
    }
}
