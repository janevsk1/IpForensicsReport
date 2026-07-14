using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Data;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}