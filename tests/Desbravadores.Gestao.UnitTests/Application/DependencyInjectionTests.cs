using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Application.Common.Behaviors;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Auth.Login;
using Desbravadores.Gestao.Application.UseCases.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Auth.Refresh;
using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using Desbravadores.Gestao.UnitTests.TestDoubles;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Desbravadores.Gestao.UnitTests.Application;

public sealed class DependencyInjectionTests
{
  [Fact]
  public void AddApplication_registers_handlers_validators_and_validation_behavior()
  {
    var services = new ServiceCollection();
    services.AddSingleton<IUsuarioRepository, FakeUsuarioRepository>();
    services.AddSingleton<IUsuarioSessaoRepository, FakeUsuarioSessaoRepository>();
    services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
    services.AddSingleton<ITokenService, FakeTokenService>();

    services.AddApplication();

    using var provider = services.BuildServiceProvider();

    Assert.NotNull(provider.GetRequiredService<LoginCommandHandler>());
    Assert.NotNull(provider.GetRequiredService<LogoutRequestCommandHandler>());
    Assert.NotNull(provider.GetRequiredService<MeQueryHandler>());
    Assert.NotNull(provider.GetRequiredService<RefreshQueryHandler>());
    Assert.NotNull(provider.GetRequiredService<CriarUsuarioCommandHandler>());
    Assert.NotNull(provider.GetRequiredService<GetAllUsuariosQueryHandler>());
    Assert.NotNull(provider.GetRequiredService<BuscaPorIdQueryHandler>());
    Assert.NotNull(provider.GetRequiredService<DeletarUsuarioCommandHandler>());
    Assert.NotNull(provider.GetRequiredService<AtualizarUsuarioCommandHandler>());

    AssertValidatorRegistered<LoginCommand>(provider);
    AssertValidatorRegistered<LogoutCommand>(provider);
    AssertValidatorRegistered<MeQuery>(provider);
    AssertValidatorRegistered<RefreshQuery>(provider);
    AssertValidatorRegistered<CriarUsuarioCommand>(provider);
    AssertValidatorRegistered<BuscaUsuarioPorIdQuery>(provider);
    AssertValidatorRegistered<DeletarUsuarioCommand>(provider);
    AssertValidatorRegistered<AtualizarUsuarioCommand>(provider);

    var behaviors = provider
      .GetServices<IPipelineBehavior<CriarUsuarioCommand, Guid>>()
      .ToList();
    Assert.Contains(behaviors, x => x.GetType() == typeof(ValidationBehavior<CriarUsuarioCommand, Guid>));
  }

  private static void AssertValidatorRegistered<TRequest>(IServiceProvider provider)
  {
    Assert.NotEmpty(provider.GetServices<IValidator<TRequest>>());
  }
}
