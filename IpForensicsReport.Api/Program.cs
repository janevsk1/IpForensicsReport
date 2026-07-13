using IpForensicsReport.Api.Data;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<
    IDbConnectionFactory,
    MySqlConnectionFactory>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

//app.MapGet("/", () => Results.Ok(new
//{
//    message = "IpForensicsReport API is running"
//}));

app.MapGet(
    "/health/database",
    async (
        IDbConnectionFactory connectionFactory,
        CancellationToken cancellationToken) =>
    {
        await using var connection =
            await connectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command =
            new MySqlCommand("SELECT 1;", connection);

        await command.ExecuteScalarAsync(cancellationToken);

        return Results.Ok(new
        {
            status = "Healthy",
            database = connection.Database
        });
    });

app.Run();