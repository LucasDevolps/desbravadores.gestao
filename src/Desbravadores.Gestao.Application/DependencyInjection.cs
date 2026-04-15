using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IValidator<CriarUsuarioRequest>, CriarUsuarioRequestValidator>();
    services.AddScoped<CriarUsuarioRequestHandler>();
    
    services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
    services.AddScoped<LoginRequestHandler>();
    
    services.AddScoped<LogoutRequestHandler>();
    services.AddScoped<MeRequestHandler>();

    services.AddScoped<MeRequestHandler>();
    services.AddScoped<IValidator<MeRequest>, MeRequestValidator>();

    services.AddScoped<GetAllUsuariosRequestHandler>();

    return services;
  }
}
