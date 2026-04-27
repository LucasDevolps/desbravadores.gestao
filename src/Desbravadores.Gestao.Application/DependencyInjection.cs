using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.Auth.Refresh;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.Application;

public static class DependencyInjection
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    /*
     *  Auth
    */ 
    // Login
    services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
    services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
    services.AddScoped<LogoutRequestCommandHandler>();

    // Logout
    services.AddScoped<IValidator<LogoutCommand>, LogoutCommandValidator>();
    services.AddValidatorsFromAssemblyContaining<LogoutCommandValidator>();
    services.AddScoped<LoginCommandHandler>();

    // Me
    services.AddScoped<IValidator<MeQuery>, MeQueryValidator>();
    services.AddValidatorsFromAssemblyContaining<MeQueryValidator>();
    services.AddScoped<MeQueryHandler>();

    // Refresh
    services.AddScoped<IValidator<RefreshQuery>, RefreshQueryValidator>();
    services.AddValidatorsFromAssemblyContaining<RefreshQueryValidator>();
    services.AddScoped<RefreshQueryHandler>();
    
    // =====================================================================
    /*
     *  Usuários
    */
    //Criar usuários - POST
    services.AddScoped<IValidator<CriarUsuarioCommand>, CriarUsuarioCommandValidator>();
    services.AddValidatorsFromAssemblyContaining<CriarUsuarioCommandValidator>();
    services.AddScoped<CriarUsuarioCommandHandler>();

    //Criar usuários - GET todos
    services.AddScoped<GetAllUsuariosQueryHandler>();

    //Criar usuários - GET por id
    services.AddScoped<IValidator<BuscaUsuarioPorIdQuery>, BuscaPorIdQueryValidator>();
    services.AddValidatorsFromAssemblyContaining<BuscaPorIdQueryValidator>();
    services.AddScoped<BuscaPorIdQueryHandler>();

    // Criar usuários - DELETE
    services.AddScoped<IValidator<DeletarUsuarioCommand>, DeletarusuarioCommandValidator>();
    services.AddValidatorsFromAssemblyContaining<DeletarusuarioCommandValidator>();
    services.AddScoped<DeletarUsuarioCommandHandler>();

    // Criar usuários - PUT - UPDATE
    services.AddScoped<IValidator<AtualizarUsuarioCommand>, AtualizarUsuarioCommandValidator>();
    services.AddValidatorsFromAssemblyContaining<AtualizarUsuarioCommandValidator>();
    services.AddScoped<AtualizarUsuarioCommandHandler>();

    
    return services;
  }
}
