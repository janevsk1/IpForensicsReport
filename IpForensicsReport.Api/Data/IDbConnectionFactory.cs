using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Data;

public interface IDbConnectionFactory
{
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}