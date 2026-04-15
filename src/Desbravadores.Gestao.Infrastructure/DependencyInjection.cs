using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
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
    services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
          Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
          ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada."))
        );

    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    services.AddScoped<IUsuarioSessaoRepository, UsuarioSessaoRepository>();
    services.AddScoped<ITokenService, TokenService>();
    services.AddScoped<IPasswordHasher, PasswordHasher>();

    return services;
  }
}
