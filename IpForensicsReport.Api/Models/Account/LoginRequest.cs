using System.ComponentModel.DataAnnotations;

namespace IpForensicsReport.Api.Models.Account
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; init; } = string.Empty;
    }
}
