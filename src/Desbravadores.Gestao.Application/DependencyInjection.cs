using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<CriarUsuarioHandler>();
    return services;
  }
}
