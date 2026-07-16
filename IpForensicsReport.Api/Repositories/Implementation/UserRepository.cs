using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.User;
using IpForensicsReport.Api.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private const int DuplicateEntryErrorNumber = 1062;

    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long?> TryCreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Users
            (
                FirstName,
                LastName,
                Email,
                PasswordHash,
                CreatedOn
            )
            VALUES
            (
                @FirstName,
                @LastName,
                @Email,
                @PasswordHash,
                @CreatedOn
            );
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters
            .Add("@FirstName", MySqlDbType.VarChar, 100)
            .Value = user.FirstName;

        command.Parameters
            .Add("@LastName", MySqlDbType.VarChar, 100)
            .Value = user.LastName;

        command.Parameters
            .Add("@Email", MySqlDbType.VarChar, 255)
            .Value = user.Email;

        command.Parameters
            .Add("@PasswordHash", MySqlDbType.VarChar, 512)
            .Value = user.PasswordHash;

        command.Parameters
            .Add("@CreatedOn", MySqlDbType.DateTime)
            .Value = user.CreatedOn;

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            return command.LastInsertedId;
        }
        catch (MySqlException exception)
            when (exception.Number == DuplicateEntryErrorNumber)
        {
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(
    string email,
    CancellationToken cancellationToken = default)
    {
        const string sql = """
        SELECT
            Id,
            FirstName,
            LastName,
            Email,
            PasswordHash
        FROM users
        WHERE email = @email
        LIMIT 1;
        """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.Add(
            "@email",
            MySqlDbType.VarChar,
            255).Value = email;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            FirstName = reader.GetString(
                reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(
                reader.GetOrdinal("LastName")),
            Email = reader.GetString(
                reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(
                reader.GetOrdinal("PasswordHash"))
        };
    }
}