using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Infrastructure.Data;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Desbravadores.Gestao.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    string connectionString =
      Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
      ?? Environment.GetEnvironmentVariable("DefaultConnectionDesbravadores")
      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

    services.AddDbContext<AppDbContext>(options =>
      options.UseNpgsql(connectionString));

    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    services.AddScoped<IUsuarioSessaoRepository, UsuarioSessaoRepository>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();

    return services;
  }
}
