using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Infrastructure.Data;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services)
  {
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
          Environment.GetEnvironmentVariable("DefaultConnectionDesbravadores") 
          ?? throw new NotImplementedException("Instancia não encontrada"))
        );
    services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    return services;
  }
}
