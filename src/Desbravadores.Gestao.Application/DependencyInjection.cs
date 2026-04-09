using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<CriarUsuarioHandler>();
    services.AddScoped<LoginRequestHandler>();
    services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
    return services;
  }
}
