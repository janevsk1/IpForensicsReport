using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Data;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
    }

    public MySqlConnection CreateConnection()
    {
        // Do not open the connection here.
        return new MySqlConnection(_connectionString);
    }
}