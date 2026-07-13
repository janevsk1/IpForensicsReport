using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Data;

public interface IDbConnectionFactory
{
    Task<MySqlConnection> CreateConnectionAsync(
        CancellationToken cancellationToken = default);
}