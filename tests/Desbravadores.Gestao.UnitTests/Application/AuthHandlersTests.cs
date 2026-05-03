using Desbravadores.Gestao.Application.UseCases.Auth.Login;
using Desbravadores.Gestao.Application.UseCases.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Auth.Refresh;
using Desbravadores.Gestao.Application.UseCases.Auth.Token;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.UnitTests.TestDoubles;

namespace Desbravadores.Gestao.UnitTests.Application;

public sealed class AuthHandlersTests
{
  [Fact]
  public async Task Login_returns_tokens_and_creates_new_session()
  {
    var usuario = UsuarioTestFactory.Create(
      uuid: Guid.NewGuid(),
      id: 42,
      nome: "Lucas",
      email: "lucas@email.com",
      senha: "stored-hash",
      role: Roles.DIRETORIA);
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(usuario);
    var sessoes = new FakeUsuarioSessaoRepository();
    var token = new TokenResult(
      "access-token",
      "refresh-token",
      "jti-token",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    var tokenService = new FakeTokenService { TokenResultado = token };
    var passwordHasher = new FakePasswordHasher { VerifyResultado = true };
    var handler = new LoginCommandHandler(usuarios, sessoes, tokenService, passwordHasher);

    var response = await handler.Handle(new LoginCommand("lucas@email.com", "123456"), CancellationToken.None);

    Assert.Equal(token, response.Token);
    Assert.Equal(token.AccessTokenExpiraEm, response.Expiracao);
    Assert.Equal("Lucas", response.Nome);
    Assert.Equal("lucas@email.com", response.Email);
    Assert.Equal(usuario, Assert.Single(tokenService.UsuariosUsados));
    Assert.Equal(42, Assert.Single(sessoes.UsuariosRevogados));
    var sessao = Assert.Single(sessoes.Sessoes);
    Assert.Equal(42, sessao.UsuarioId);
    Assert.Equal("jti-token", sessao.Jti);
    Assert.Equal("refresh-token", sessao.RefreshToken);
    Assert.Equal(1, sessoes.SaveChangesChamadas);
    Assert.Equal(("123456", "stored-hash"), Assert.Single(passwordHasher.Verificacoes));
  }

  [Fact]
  public async Task Login_throws_when_user_is_not_found()
  {
    var handler = new LoginCommandHandler(
      new FakeUsuarioRepository(),
      new FakeUsuarioSessaoRepository(),
      new FakeTokenService(),
      new FakePasswordHasher());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new LoginCommand("missing@email.com", "123456"), CancellationToken.None));
  }

  [Fact]
  public async Task Login_throws_when_password_is_invalid()
  {
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(UsuarioTestFactory.Create(email: "lucas@email.com", senha: "stored-hash"));
    var sessoes = new FakeUsuarioSessaoRepository();
    var handler = new LoginCommandHandler(
      usuarios,
      sessoes,
      new FakeTokenService(),
      new FakePasswordHasher { VerifyResultado = false });

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new LoginCommand("lucas@email.com", "wrong"), CancellationToken.None));

    Assert.Empty(sessoes.Sessoes);
    Assert.Empty(sessoes.UsuariosRevogados);
  }

  [Fact]
  public async Task Logout_revokes_existing_session()
  {
    var sessao = new UsuarioSessao(
      10,
      "jti",
      "refresh",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    var sessoes = new FakeUsuarioSessaoRepository();
    sessoes.Sessoes.Add(sessao);
    var handler = new LogoutRequestCommandHandler(sessoes);

    var result = await handler.Handle(new LogoutCommand("jti"), CancellationToken.None);

    Assert.Equal(MediatR.Unit.Value, result);
    Assert.True(sessao.Revogado);
    Assert.NotNull(sessao.DataRevogacao);
    Assert.Equal(1, sessoes.SaveChangesChamadas);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public async Task Logout_throws_when_jti_is_blank(string jti)
  {
    var handler = new LogoutRequestCommandHandler(new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new LogoutCommand(jti), CancellationToken.None));
  }

  [Fact]
  public async Task Logout_throws_when_session_is_not_found()
  {
    var handler = new LogoutRequestCommandHandler(new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new LogoutCommand("missing"), CancellationToken.None));
  }

  [Fact]
  public async Task Me_returns_user_when_session_is_active()
  {
    var usuario = UsuarioTestFactory.Create(id: 12, nome: "Lucas");
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(usuario);
    var sessoes = new FakeUsuarioSessaoRepository { ExistsActiveSessionResultado = true };
    var handler = new MeQueryHandler(sessoes, usuarios);

    var dto = await handler.Handle(new MeQuery(usuario.Uuid.ToString(), "jti"), CancellationToken.None);

    Assert.Equal(usuario.Uuid, dto.Id);
    Assert.Equal("Lucas", dto.Nome);
  }

  [Fact]
  public async Task Me_throws_when_sub_is_not_a_guid()
  {
    var handler = new MeQueryHandler(new FakeUsuarioSessaoRepository(), new FakeUsuarioRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new MeQuery("not-guid", "jti"), CancellationToken.None));
  }

  [Fact]
  public async Task Me_throws_when_jti_is_blank()
  {
    var handler = new MeQueryHandler(new FakeUsuarioSessaoRepository(), new FakeUsuarioRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new MeQuery(Guid.NewGuid().ToString(), " "), CancellationToken.None));
  }

  [Fact]
  public async Task Me_throws_when_user_is_not_found()
  {
    var handler = new MeQueryHandler(new FakeUsuarioSessaoRepository(), new FakeUsuarioRepository());

    await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      handler.Handle(new MeQuery(Guid.NewGuid().ToString(), "jti"), CancellationToken.None));
  }

  [Fact]
  public async Task Me_throws_when_session_is_not_active()
  {
    var usuario = UsuarioTestFactory.Create(id: 12);
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(usuario);
    var sessoes = new FakeUsuarioSessaoRepository { ExistsActiveSessionResultado = false };
    var handler = new MeQueryHandler(sessoes, usuarios);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new MeQuery(usuario.Uuid.ToString(), "jti"), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_returns_new_token_and_session_when_current_session_is_active()
  {
    var usuario = UsuarioTestFactory.Create(id: 77, nome: "Lucas", email: "lucas@email.com");
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(usuario);
    var sessoes = new FakeUsuarioSessaoRepository { ExistsActiveSessionResultado = true };
    var token = new TokenResult(
      "new-access",
      "new-refresh",
      "new-jti",
      DateTime.UtcNow.AddMinutes(30),
      DateTime.UtcNow.AddDays(10));
    var tokenService = new FakeTokenService { TokenResultado = token };
    var handler = new RefreshQueryHandler(tokenService, usuarios, sessoes);

    var response = await handler.Handle(new RefreshQuery(usuario.Uuid.ToString(), "old-jti"), CancellationToken.None);

    Assert.Equal(token, response.Token);
    Assert.Equal("Lucas", response.Nome);
    Assert.Equal("lucas@email.com", response.Email);
    Assert.Equal(77, Assert.Single(sessoes.UsuariosRevogados));
    var sessao = Assert.Single(sessoes.Sessoes);
    Assert.Equal("new-jti", sessao.Jti);
    Assert.Equal("new-refresh", sessao.RefreshToken);
    Assert.Equal(1, sessoes.SaveChangesChamadas);
  }

  [Fact]
  public async Task Refresh_throws_when_sub_is_not_a_guid()
  {
    var handler = new RefreshQueryHandler(
      new FakeTokenService(),
      new FakeUsuarioRepository(),
      new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new RefreshQuery("not-guid", "jti"), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_throws_when_jti_is_blank()
  {
    var handler = new RefreshQueryHandler(
      new FakeTokenService(),
      new FakeUsuarioRepository(),
      new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new RefreshQuery(Guid.NewGuid().ToString(), " "), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_throws_when_user_is_not_found()
  {
    var handler = new RefreshQueryHandler(
      new FakeTokenService(),
      new FakeUsuarioRepository(),
      new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      handler.Handle(new RefreshQuery(Guid.NewGuid().ToString(), "jti"), CancellationToken.None));
  }

  [Fact]
  public async Task Refresh_throws_when_session_is_not_active()
  {
    var usuario = UsuarioTestFactory.Create(id: 77);
    var usuarios = new FakeUsuarioRepository();
    usuarios.Usuarios.Add(usuario);
    var handler = new RefreshQueryHandler(
      new FakeTokenService(),
      usuarios,
      new FakeUsuarioSessaoRepository { ExistsActiveSessionResultado = false });

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
      handler.Handle(new RefreshQuery(usuario.Uuid.ToString(), "jti"), CancellationToken.None));
  }
}
