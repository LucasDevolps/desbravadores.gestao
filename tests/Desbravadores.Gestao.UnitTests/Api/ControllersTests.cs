using System.Security.Claims;
using Desbravadores.Gestao.Api.Controllers;
using Desbravadores.Gestao.Application.DTOs;
using Desbravadores.Gestao.Application.UseCases.Auth.Login;
using Desbravadores.Gestao.Application.UseCases.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Auth.Refresh;
using Desbravadores.Gestao.Application.UseCases.Auth.Token;
using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.UnitTests.Api;

public sealed class ControllersTests
{
  [Fact]
  public async Task Auth_login_sends_command_and_returns_ok()
  {
    var mediator = new FakeMediator();
    var token = new TokenResult("access", "refresh", "jti", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));
    var response = new LoginResponse(token, token.AccessTokenExpiraEm, "Lucas", "lucas@email.com");
    mediator.Respond(response);
    var controller = new AuthController(mediator);
    var command = new LoginCommand("lucas@email.com", "123456");

    var result = await controller.Login(command, CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(response, ok.Value);
    Assert.Same(command, Assert.Single(mediator.Requests));
  }

  [Fact]
  public async Task Auth_logout_reads_jti_claim_and_returns_no_content()
  {
    var mediator = new FakeMediator();
    var controller = new AuthController(mediator);
    SetUser(controller, sub: Guid.NewGuid().ToString(), jti: "jti-token");

    var result = await controller.Logout(CancellationToken.None);

    Assert.IsType<NoContentResult>(result);
    var command = Assert.IsType<LogoutCommand>(Assert.Single(mediator.Requests));
    Assert.Equal("jti-token", command.Jti);
  }

  [Fact]
  public async Task Auth_me_reads_claims_and_returns_current_user()
  {
    var mediator = new FakeMediator();
    var dto = new UsuarioDTO { Id = Guid.NewGuid(), Nome = "Lucas", Email = "lucas@email.com" };
    mediator.Respond(dto);
    var controller = new AuthController(mediator);
    SetUser(controller, dto.Id.ToString(), "jti-token");

    var result = await controller.Me(CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(dto, ok.Value);
    var query = Assert.IsType<MeQuery>(Assert.Single(mediator.Requests));
    Assert.Equal(dto.Id.ToString(), query.Sub);
    Assert.Equal("jti-token", query.Jti);
  }

  [Fact]
  public async Task Auth_refresh_reads_claims_and_returns_new_token()
  {
    var mediator = new FakeMediator();
    var token = new TokenResult("access", "refresh", "jti", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));
    var response = new RefreshResponse(token, token.AccessTokenExpiraEm, "Lucas", "lucas@email.com");
    mediator.Respond(response);
    var controller = new AuthController(mediator);
    var sub = Guid.NewGuid().ToString();
    SetUser(controller, sub, "jti-token");

    var result = await controller.Refresh(CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(response, ok.Value);
    var query = Assert.IsType<RefreshQuery>(Assert.Single(mediator.Requests));
    Assert.Equal(sub, query.Sub);
    Assert.Equal("jti-token", query.Jti);
  }

  [Fact]
  public async Task Usuarios_criar_usuario_sends_command_and_returns_created_at_obter_por_id()
  {
    var mediator = new FakeMediator();
    var id = Guid.NewGuid();
    mediator.Respond(id);
    var controller = new UsuariosController(mediator);
    var command = new CriarUsuarioCommand("Lucas", "lucas@email.com", "123456", "DIRETORIA");

    var result = await controller.CriarUsuario(command, CancellationToken.None);

    var created = Assert.IsType<CreatedAtActionResult>(result);
    Assert.Equal(nameof(UsuariosController.ObterPorId), created.ActionName);
    Assert.Equal(id, created.RouteValues!["id"]);
    Assert.Same(command, Assert.Single(mediator.Requests));
  }

  [Fact]
  public async Task Usuarios_get_all_sends_query_and_returns_ok()
  {
    var mediator = new FakeMediator();
    IEnumerable<UsuarioDTO> usuarios =
    [
      new UsuarioDTO { Id = Guid.NewGuid(), Nome = "Lucas", Email = "lucas@email.com" }
    ];
    mediator.Respond(usuarios);
    var controller = new UsuariosController(mediator);

    var result = await controller.GetAll(CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(usuarios, ok.Value);
    Assert.IsType<GetAllUsuariosQuery>(Assert.Single(mediator.Requests));
  }

  [Fact]
  public async Task Usuarios_obter_por_id_sends_query_and_returns_ok()
  {
    var mediator = new FakeMediator();
    var id = Guid.NewGuid();
    var dto = new UsuarioDTO { Id = id, Nome = "Lucas", Email = "lucas@email.com" };
    mediator.Respond(dto);
    var controller = new UsuariosController(mediator);

    var result = await controller.ObterPorId(id, CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(dto, ok.Value);
    var query = Assert.IsType<BuscaUsuarioPorIdQuery>(Assert.Single(mediator.Requests));
    Assert.Equal(id, query.Id);
  }

  [Fact]
  public async Task Usuarios_deletar_usuario_sends_command_and_returns_no_content()
  {
    var mediator = new FakeMediator();
    var id = Guid.NewGuid();
    var controller = new UsuariosController(mediator);

    var result = await controller.DeletarUsuario(id, CancellationToken.None);

    Assert.IsType<NoContentResult>(result);
    var command = Assert.IsType<DeletarUsuarioCommand>(Assert.Single(mediator.Requests));
    Assert.Equal(id, command.Id);
  }

  [Fact]
  public async Task Usuarios_atualizar_usuario_replaces_body_uuid_with_route_uuid()
  {
    var mediator = new FakeMediator();
    var routeId = Guid.NewGuid();
    var dto = new UsuarioDTO
    {
      Id = routeId,
      Nome = "Maria",
      Email = "maria@email.com",
      Roles = Roles.SECRETARIA
    };
    mediator.Respond(dto);
    var controller = new UsuariosController(mediator);
    var body = new AtualizarUsuarioCommand(
      Guid.NewGuid(),
      "Maria",
      "maria@email.com",
      "123456",
      "SECRETARIA");

    var result = await controller.AtualizarUsuario(routeId, body, CancellationToken.None);

    var ok = Assert.IsType<OkObjectResult>(result);
    Assert.Same(dto, ok.Value);
    var command = Assert.IsType<AtualizarUsuarioCommand>(Assert.Single(mediator.Requests));
    Assert.Equal(routeId, command.Uuid);
    Assert.Equal(body.Nome, command.Nome);
    Assert.Equal(body.Email, command.Email);
    Assert.Equal(body.Senha, command.Senha);
    Assert.Equal(body.Roles, command.Roles);
  }

  private static void SetUser(Controller controller, string sub, string jti)
  {
    var identity = new ClaimsIdentity(
      [
        new Claim("sub", sub),
        new Claim("jti", jti)
      ],
      "Test");

    controller.ControllerContext = new ControllerContext
    {
      HttpContext = new DefaultHttpContext
      {
        User = new ClaimsPrincipal(identity)
      }
    };
  }
}
