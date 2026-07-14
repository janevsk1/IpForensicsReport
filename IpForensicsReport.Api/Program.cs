using IpForensicsReport.Api.Data;
using IpForensicsReport.Api.Models.User;
using IpForensicsReport.Api.Repositories.Implementations;
using IpForensicsReport.Api.Repositories.Interfaces;
using IpForensicsReport.Api.Services;
using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using System.Text;
using IpForensicsReport.Api.Configuration;
using IpForensicsReport.Api.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<
    IDbConnectionFactory,
    MySqlConnectionFactory>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

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
        await using var connection = connectionFactory.CreateConnection();

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