using System.Reflection;
using Desbravadores.Gestao.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Desbravadores.Gestao.UnitTests.Api;

public sealed class AuthorizationMetadataTests
{
  [Fact]
  public void UsuariosController_is_authorized_and_uses_expected_policies()
  {
    Assert.NotNull(typeof(UsuariosController).GetCustomAttribute<AuthorizeAttribute>());
    AssertPolicy(nameof(UsuariosController.CriarUsuario), "MasterOnly");
    AssertPolicy(nameof(UsuariosController.GetAll), "Financeiro");
    AssertPolicy(nameof(UsuariosController.ObterPorId), "MasterOnly");
    AssertPolicy(nameof(UsuariosController.DeletarUsuario), "MasterOnly");
    AssertPolicy(nameof(UsuariosController.AtualizarUsuario), "MasterOnly");
  }

  [Fact]
  public void AuthController_protects_authenticated_endpoints_but_not_login()
  {
    Assert.Empty(GetAuthorizeAttributes(nameof(AuthController.Login)));
    Assert.NotEmpty(GetAuthorizeAttributes(nameof(AuthController.Logout)));
    Assert.NotEmpty(GetAuthorizeAttributes(nameof(AuthController.Me)));
    Assert.NotEmpty(GetAuthorizeAttributes(nameof(AuthController.Refresh)));
  }

  private static void AssertPolicy(string methodName, string expectedPolicy)
  {
    var policies = GetAuthorizeAttributes(methodName)
      .Select(x => x.Policy)
      .ToList();

    Assert.Contains(expectedPolicy, policies);
  }

  private static IReadOnlyCollection<AuthorizeAttribute> GetAuthorizeAttributes(string methodName)
  {
    var method = typeof(UsuariosController).GetMethod(methodName)
      ?? typeof(AuthController).GetMethod(methodName)
      ?? throw new MissingMethodException(methodName);

    return method.GetCustomAttributes<AuthorizeAttribute>().ToList();
  }
}
