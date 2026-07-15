using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.Reports;
using IpForensicsReport.Api.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data.Common;

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

        public async Task<IReadOnlyList<EncryptedReportRecord>> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT
                Id,
                EncryptedPayload,
                EncryptionNonce,
                AuthenticationTag,
                CreatedOn
            FROM IpForensicsReports
            WHERE UserId = @userId
            ORDER BY CreatedOn DESC;
            """;

            await using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync(cancellationToken);

            await using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.Add(
                "@userId",
                MySqlDbType.Int64
            ).Value = userId;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            var reports = new List<EncryptedReportRecord>();

            while (await reader.ReadAsync(cancellationToken))
            {
                reports.Add(MapEncryptedReport(reader));
            }

            return reports;
        }

        private static EncryptedReportRecord MapEncryptedReport(
            DbDataReader reader)
        {
            return new EncryptedReportRecord
            {
                Id = Convert.ToInt64(
                    reader["Id"]),

                EncryptedPayload =
                    (byte[])reader["EncryptedPayload"],

                EncryptionNonce =
                    (byte[])reader["EncryptionNonce"],

                AuthenticationTag =
                    (byte[])reader["AuthenticationTag"],

                CreatedOnUtc = Convert.ToDateTime(
                    reader["CreatedOn"])
            };
        }
    }
}
