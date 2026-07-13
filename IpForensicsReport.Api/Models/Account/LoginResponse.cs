namespace IpForensicsReport.Api.Models.Account
{
    public sealed class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string TokenType { get; set; } = "Bearer";

        public DateTime ExpiresAtUtc { get; set; }

        public AuthenticatedUserResponse User { get; set; } = new();
    }
}
