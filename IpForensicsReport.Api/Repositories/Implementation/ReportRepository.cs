using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.Reports;
using IpForensicsReport.Api.Repositories.Interfaces;
using MySql.Data.MySqlClient;

namespace IpForensicsReport.Api.Repositories.Implementation
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReportRepository(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<long> CreateAsync(
            CreateReportRecord report,
            CancellationToken cancellationToken)
        {
            const string sql = """
            INSERT INTO IpForensicsReports
            (
                UserId,
                EncryptedPayload,
                EncryptionNonce,
                AuthenticationTag,
                CreatedOn
            )
            VALUES
            (
                @userId,
                @encryptedPayload,
                @encryptionNonce,
                @authenticationTag,
                @createdOn
            );
            """;

            //TODO: Use shared/common method to create connection and command objects to avoid code duplication
            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.Add(
                "@userId",
                MySqlDbType.Int64
            ).Value = report.UserId;

            command.Parameters.Add(
                "@encryptedPayload",
                MySqlDbType.LongBlob
            ).Value = report.EncryptedPayload;

            command.Parameters.Add(
                "@encryptionNonce",
                MySqlDbType.VarBinary
            ).Value = report.EncryptionNonce;

            command.Parameters.Add(
                "@authenticationTag",
                MySqlDbType.VarBinary
            ).Value = report.AuthenticationTag;

            command.Parameters.Add(
                "@createdOn",
                MySqlDbType.DateTime
            ).Value = report.CreatedOnUtc;

            await command.ExecuteNonQueryAsync(
                cancellationToken);

            return command.LastInsertedId;
        }
    }
}
