using IpForensicsReport.Api.Configuration;
using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.User;
using IpForensicsReport.Api.Repositories.Implementation;
using IpForensicsReport.Api.Repositories.Implementations;
using IpForensicsReport.Api.Repositories.Interfaces;
using IpForensicsReport.Api.Services;
using IpForensicsReport.Api.Services.Authentication;
using IpForensicsReport.Api.Services.Reports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Database
builder.Services.AddSingleton<
    IDbConnectionFactory,
    MySqlConnectionFactory>();

// Account services
builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IAccountService,
    AccountService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

// JWT configuration
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(
        JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException(
        "JWT signing key is not configured.");
}

if (Encoding.UTF8.GetByteCount(jwtOptions.SigningKey) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key must be at least 32 bytes.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException(
        "JWT issuer is not configured.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException(
        "JWT audience is not configured.");
}

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SigningKey)),

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ClockSkew = TimeSpan.FromSeconds(30),

                NameClaimType =
                    JwtRegisteredClaimNames.Email
            };
    });

builder.Services.AddAuthorization();

// External API configuration
builder.Services.Configure<IpApiOptions>(
    builder.Configuration.GetSection(
        IpApiOptions.SectionName));

builder.Services.Configure<AbuseIpDbOptions>(
    builder.Configuration.GetSection(
        AbuseIpDbOptions.SectionName));

builder.Services.Configure<ReportEncryptionOptions>(
    builder.Configuration.GetSection(
        ReportEncryptionOptions.SectionName));

// External API clients
builder.Services.AddHttpClient<
    IIpApiClient,
    IpApiClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<IpApiOptions>>()
            .Value;

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException(
                "ip-api base URL is not configured.");
        }

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(10);
    });

builder.Services.AddHttpClient<
    IAbuseIpDbClient,
    AbuseIpDbClient>(
    (serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<AbuseIpDbOptions>>()
            .Value;

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException(
                "AbuseIPDB base URL is not configured.");
        }

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(10);
    });

// Report services
builder.Services.AddSingleton<
    IReportEncryptionService,
    ReportEncryptionService>();

builder.Services.AddScoped<
    IReportRepository,
    ReportRepository>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet(
    "/health/database",
    async (
        IDbConnectionFactory connectionFactory,
        CancellationToken cancellationToken) =>
    {
        await using var connection =
            connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new MySqlCommand(
                "SELECT 1;",
                connection);

        await command.ExecuteScalarAsync(
            cancellationToken);

        return Results.Ok(new
        {
            status = "Healthy",
            database = connection.Database
        });
    });

app.Run();