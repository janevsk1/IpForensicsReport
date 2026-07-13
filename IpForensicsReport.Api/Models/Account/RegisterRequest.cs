using System.ComponentModel.DataAnnotations;

namespace IpForensicsReport.Api.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; init; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; init; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; init; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;
    }
}
