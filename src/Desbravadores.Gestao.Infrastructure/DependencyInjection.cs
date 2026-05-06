using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Infrastructure.Data;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Desbravadores.Gestao.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Desbravadores.Gestao.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration? configuration = null)
  {
    string connectionString = ResolveConnectionString(configuration);

    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(connectionString));

    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    services.AddScoped<IUsuarioSessaoRepository, UsuarioSessaoRepository>();
    services.AddScoped<IApiRequestLogRepository, ApiRequestLogRepository>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();

    return services;
  }

  private static string ResolveConnectionString(IConfiguration? configuration)
  {
    string? connectionString = FirstConfigured(
      configuration?["DATABASE_URL"],
      Environment.GetEnvironmentVariable("DATABASE_URL"),
      configuration?.GetConnectionString("DefaultConnection"),
      Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
      Environment.GetEnvironmentVariable("DefaultConnectionDesbravadores"));

    if (connectionString is null)
    {
      throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    return NormalizePostgresConnectionString(connectionString);
  }

  private static string? FirstConfigured(params string?[] values)
  {
    return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
  }

  private static string NormalizePostgresConnectionString(string connectionString)
  {
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var databaseUri) ||
        databaseUri.Scheme is not ("postgres" or "postgresql"))
    {
      return connectionString;
    }

    if (string.IsNullOrWhiteSpace(databaseUri.Host) || databaseUri.AbsolutePath.Length <= 1)
    {
      throw new InvalidOperationException("DATABASE_URL inválida para PostgreSQL.");
    }

    var credentials = databaseUri.UserInfo.Split(':', 2);
    var npgsqlConnection = new NpgsqlConnectionStringBuilder
    {
      Host = databaseUri.Host,
      Port = databaseUri.IsDefaultPort ? 5432 : databaseUri.Port,
      Database = Uri.UnescapeDataString(databaseUri.AbsolutePath.TrimStart('/')),
      SslMode = SslMode.Require
    };

    if (credentials.Length > 0 && !string.IsNullOrWhiteSpace(credentials[0]))
    {
      npgsqlConnection.Username = Uri.UnescapeDataString(credentials[0]);
    }

    if (credentials.Length > 1)
    {
      npgsqlConnection.Password = Uri.UnescapeDataString(credentials[1]);
    }

    return npgsqlConnection.ConnectionString;
  }
}
