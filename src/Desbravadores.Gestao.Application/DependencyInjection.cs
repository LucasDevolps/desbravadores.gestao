using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IValidator<CriarUsuarioRequest>, CriarUsuarioValidation>();
    services.AddScoped<CriarUsuarioRequestHandler>();
    
    services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
    services.AddScoped<LoginRequestHandler>();
    
    services.AddScoped<LogoutRequestHandler>();
    services.AddScoped<MeRequestHandler>();
    
    return services;
  }
}
