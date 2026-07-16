using IpForensicsReport.Api.Models.User;

namespace IpForensicsReport.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<long?> TryCreateAsync(User user, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
