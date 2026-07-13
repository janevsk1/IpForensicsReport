using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.User;
using IpForensicsReport.Api.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Repositories.Implementations;

public sealed class UserRepository : IUserRepository
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
                Password,
                CreatedOn
            )
            VALUES
            (
                @FirstName,
                @LastName,
                @Email,
                @Password,
                @CreatedOn
            );
            """;

        await using var connection =
            await _connectionFactory.CreateConnectionAsync(cancellationToken);

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
            .Add("@Password", MySqlDbType.VarChar, 512)
            .Value = user.Password;

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
            Password
        FROM users
        WHERE email = @email
        LIMIT 1;
        """;

        await using var connection = await _connectionFactory.CreateConnectionAsync();

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
            Password = reader.GetString(
                reader.GetOrdinal("Password"))
        };
    }

    public async Task UpdatePasswordHashAsync(long userId, string passwordHash, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE users
            SET password_hash = @passwordHash
            WHERE id = @userId;
            """;

        await using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.OpenAsync(cancellationToken);

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters
            .Add("@passwordHash", MySqlDbType.VarChar, 500)
            .Value = passwordHash;

        command.Parameters
            .Add("@userId", MySqlDbType.Int64)
            .Value = userId;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}