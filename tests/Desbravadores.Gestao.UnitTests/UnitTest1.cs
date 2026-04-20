using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;

namespace Desbravadores.Gestao.UnitTests;

public class LoginCommandValidatorTests
{
  [Fact]
  public void Deve_invalidar_quando_email_ou_senha_vazios()
  {
    var validator = new LoginCommandValidator();

    var result = validator.Validate(new LoginCommand("", ""));

    Assert.False(result.IsValid);
    Assert.NotEmpty(result.Errors);
  }

  [Fact]
  public void Deve_validar_login_com_dados_validos()
  {
    var validator = new LoginCommandValidator();

    var result = validator.Validate(new LoginCommand("user@email.com", "123456"));

    Assert.True(result.IsValid);
  }
}

public class CriarUsuarioCommandValidatorTests
{
  [Fact]
  public void Deve_invalidar_role_invalida()
  {
    var validator = new CriarUsuarioCommandValidator();

    var result = validator.Validate(new CriarUsuarioCommand("Ana", "ana@email.com", "123456", "INVALIDA"));

    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Role inválida."));
  }

  [Fact]
  public void Deve_validar_usuario_com_role_existente()
  {
    var validator = new CriarUsuarioCommandValidator();

    var result = validator.Validate(new CriarUsuarioCommand("Ana", "ana@email.com", "123456", Roles.DIRETORIA.ToString()));

    Assert.True(result.IsValid);
  }
}

public class LoginCommandHandlerTests
{
  [Fact]
  public async Task Deve_gerar_token_e_persistir_sessao_no_login_valido()
  {
    var usuario = new Usuario("Maria", "maria@email.com", "HASH", Roles.DIRETORIA);
    var usuarioRepo = new FakeUsuarioRepository { UsuarioByEmail = usuario };
    var sessaoRepo = new FakeUsuarioSessaoRepository();
    var token = new TokenResult("token", "refresh", "jti-123", DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddDays(7));
    var tokenService = new FakeTokenService(token);
    var passwordHasher = new FakePasswordHasher(true, "HASHED");
    var handler = new LoginCommandHandler(usuarioRepo, sessaoRepo, tokenService, passwordHasher);

    var response = await handler.Handle(new LoginCommand("maria@email.com", "123456"), CancellationToken.None);

    Assert.Equal("maria@email.com", response.Email);
    Assert.Equal("jti-123", response.Token.Jti);
    Assert.True(sessaoRepo.RevokeAllCalled);
    Assert.True(sessaoRepo.SaveChangesCalled);
    Assert.NotNull(sessaoRepo.AddedSession);
    Assert.Equal(usuario.Id, sessaoRepo.AddedSession!.UsuarioId);
  }

  [Fact]
  public async Task Deve_falhar_quando_senha_invalida()
  {
    var usuarioRepo = new FakeUsuarioRepository
    {
      UsuarioByEmail = new Usuario("Maria", "maria@email.com", "HASH", Roles.DIRETORIA)
    };

    var handler = new LoginCommandHandler(
      usuarioRepo,
      new FakeUsuarioSessaoRepository(),
      new FakeTokenService(new TokenResult("token", "refresh", "jti-123", DateTime.UtcNow, DateTime.UtcNow)),
      new FakePasswordHasher(false, "HASHED"));

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new LoginCommand("maria@email.com", "senhaErrada"), CancellationToken.None));
  }
}

public class MeQueryHandlerTests
{
  [Fact]
  public async Task Deve_retornar_usuario_quando_sub_e_jti_validos_com_sessao_ativa()
  {
    var usuario = new Usuario(Guid.NewGuid(), "João", "joao@email.com", "hash", Roles.SECRETARIA);
    var usuarioRepo = new FakeUsuarioRepository { UsuarioByUuid = usuario };
    var sessaoRepo = new FakeUsuarioSessaoRepository { ExistsActiveSessionResult = true };
    var handler = new MeQueryHandler(sessaoRepo, usuarioRepo);

    var response = await handler.Handle(new MeQuery(usuario.Uuid.ToString(), "jti-ativa"), CancellationToken.None);

    Assert.Equal(usuario.Uuid, response.Id);
    Assert.Equal(usuario.Email, response.Email);
  }

  [Fact]
  public async Task Deve_falhar_quando_jti_nao_esta_ativo()
  {
    var usuario = new Usuario(Guid.NewGuid(), "João", "joao@email.com", "hash", Roles.SECRETARIA);
    var usuarioRepo = new FakeUsuarioRepository { UsuarioByUuid = usuario };
    var sessaoRepo = new FakeUsuarioSessaoRepository { ExistsActiveSessionResult = false };
    var handler = new MeQueryHandler(sessaoRepo, usuarioRepo);

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new MeQuery(usuario.Uuid.ToString(), "jti-revogado"), CancellationToken.None));
  }
}

public class LogoutRequestCommandHandlerTests
{
  [Fact]
  public async Task Deve_revogar_sessao_no_logout()
  {
    var sessao = new UsuarioSessao(1, "jti-123", "refresh", DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow.AddDays(7));
    var sessaoRepo = new FakeUsuarioSessaoRepository { SessionByJti = sessao };
    var handler = new LogoutRequestCommandHandler(sessaoRepo);

    await handler.Handle(new LogoutCommand("jti-123"), CancellationToken.None);

    Assert.True(sessao.Revogado);
    Assert.True(sessaoRepo.SaveChangesCalled);
  }

  [Fact]
  public async Task Deve_falhar_quando_jti_nao_existir()
  {
    var handler = new LogoutRequestCommandHandler(new FakeUsuarioSessaoRepository());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(new LogoutCommand("jti-inexistente"), CancellationToken.None));
  }
}

public sealed class FakeUsuarioRepository : IUsuarioRepository
{
  public Usuario? UsuarioByEmail { get; set; }
  public Usuario? UsuarioByUuid { get; set; }

  public Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;

  public Task DeletarUsuarioAsync(Guid id, CancellationToken cancellationToken) => Task.CompletedTask;

  public Task<IEnumerable<UsuarioDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    => Task.FromResult<IEnumerable<UsuarioDTO>>(Array.Empty<UsuarioDTO>());

  public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    => Task.FromResult(UsuarioByEmail);

  public Task<UsuarioDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => Task.FromResult<UsuarioDTO?>(null);

  public Task<Usuario?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    => Task.FromResult(UsuarioByUuid);

  public Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default) => Task.CompletedTask;

  public Task<UsuarioDTO> UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    => Task.FromResult(new UsuarioDTO().FromEntity(usuario));
}

public sealed class FakeUsuarioSessaoRepository : IUsuarioSessaoRepository
{
  public bool RevokeAllCalled { get; private set; }
  public bool SaveChangesCalled { get; private set; }
  public UsuarioSessao? AddedSession { get; private set; }
  public UsuarioSessao? SessionByJti { get; set; }
  public bool ExistsActiveSessionResult { get; set; }

  public Task AddAsync(UsuarioSessao sessao, CancellationToken cancellationToken = default)
  {
    AddedSession = sessao;
    return Task.CompletedTask;
  }

  public Task<bool> ExistsActiveSessionAsync(long usuarioId, string jti, CancellationToken cancellationToken = default)
    => Task.FromResult(ExistsActiveSessionResult);

  public Task<UsuarioSessao?> GetByJtiAsync(string jti, CancellationToken cancellationToken = default)
    => Task.FromResult(SessionByJti);

  public Task<UsuarioSessao?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    => Task.FromResult<UsuarioSessao?>(null);

  public Task RevokeAllActiveByUsuarioIdAsync(long usuarioId, CancellationToken cancellationToken = default)
  {
    RevokeAllCalled = true;
    return Task.CompletedTask;
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    SaveChangesCalled = true;
    return Task.CompletedTask;
  }
}

public sealed class FakeTokenService : ITokenService
{
  private readonly TokenResult _tokenResult;

  public FakeTokenService(TokenResult tokenResult)
  {
    _tokenResult = tokenResult;
  }

  public Task<TokenResult> GenerateToken(Usuario usuario) => Task.FromResult(_tokenResult);
}

public sealed class FakePasswordHasher : IPasswordHasher
{
  private readonly bool _verifyResult;
  private readonly string _hashResult;

  public FakePasswordHasher(bool verifyResult, string hashResult)
  {
    _verifyResult = verifyResult;
    _hashResult = hashResult;
  }

  public Task<string> HashAsync(string password, CancellationToken cancellationToken = default)
    => Task.FromResult(_hashResult);

  public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken = default)
    => Task.FromResult(_verifyResult);
}
