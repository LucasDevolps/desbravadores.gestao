using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    services.AddScoped<IValidator<CriarUsuarioCommand>, CriarUsuarioCommandValidator>();
    services.AddScoped<CriarUsuarioCommandHandler>();
    
    services.AddScoped<IValidator<LoginRequestQuery>, LoginRequestValidator>();
    services.AddScoped<LoginRequestQueryHandler>();
    
    services.AddScoped<LogoutRequestCommandHandler>();
    services.AddScoped<MeQueryHandler>();

    services.AddScoped<MeQueryHandler>();
    services.AddScoped<IValidator<MeQuery>, MeQueryValidator>();

    services.AddScoped<GetAllUsuariosQueryHandler>();
    services.AddScoped<BuscaPorIdRequestHandler>();

    return services;
  }
}
