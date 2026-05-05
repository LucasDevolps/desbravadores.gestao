using System.Collections.Generic;
using System.Reflection;
using Desbravadores.Gestao.Application.DTOs;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Auth.Token;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.UnitTests.TestDoubles;

internal static class UsuarioTestFactory
{
  public static Usuario Create(
    Guid? uuid = null,
    int id = 1,
    string nome = "Lucas",
    string email = "lucas@email.com",
    string senha = "hash",
    Roles role = Roles.DESBRAVADOR)
  {
    var usuario = new Usuario(uuid ?? Guid.NewGuid(), nome, email, senha, role);
    SetId(usuario, id);
    return usuario;
  }

  public static void SetId(Usuario usuario, int id)
  {
    typeof(Usuario)
      .GetProperty(nameof(Usuario.Id), BindingFlags.Instance | BindingFlags.Public)!
      .SetValue(usuario, id);
  }
}

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
  public List<Usuario> Usuarios { get; } = [];
  public List<Usuario> UsuariosAdicionados { get; } = [];
  public List<Guid> UsuariosDeletados { get; } = [];
  public int SaveChangesChamadas { get; private set; }

  public Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    var emailNormalizado = email.Trim().ToLowerInvariant();
    var usuario = Usuarios.FirstOrDefault(x => x.Email == emailNormalizado);
    return Task.FromResult(usuario);
  }

  public Task<UsuarioDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var usuario = Usuarios.FirstOrDefault(x => x.Uuid == id);
    return Task.FromResult(usuario is null ? null : new UsuarioDTO().FromEntity(usuario));
  }

  public Task<Usuario?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(Usuarios.FirstOrDefault(x => x.Uuid == uuid));
  }

  public Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
  {
    Usuarios.Add(usuario);
    UsuariosAdicionados.Add(usuario);
    return Task.CompletedTask;
  }

  public Task<IEnumerable<UsuarioDTO>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return Task.FromResult(Usuarios.Select(x => new UsuarioDTO().FromEntity(x)).AsEnumerable());
  }

  public Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default)
  {
    Usuarios.RemoveAll(x => x.Uuid == usuario.Uuid);
    return Task.CompletedTask;
  }

  public Task DeletarUsuarioAsync(Guid id, CancellationToken cancellationToken)
  {
    UsuariosDeletados.Add(id);
    Usuarios.RemoveAll(x => x.Uuid == id);
    return Task.CompletedTask;
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    SaveChangesChamadas++;
    return Task.CompletedTask;
  }
}

internal sealed class FakeUsuarioSessaoRepository : IUsuarioSessaoRepository
{
  public List<UsuarioSessao> Sessoes { get; } = [];
  public List<long> UsuariosRevogados { get; } = [];
  public int AddChamadas { get; private set; }
  public int SaveChangesChamadas { get; private set; }
  public bool? ExistsActiveSessionResultado { get; set; }

  public Task AddAsync(UsuarioSessao sessao, CancellationToken cancellationToken = default)
  {
    AddChamadas++;
    Sessoes.Add(sessao);
    return Task.CompletedTask;
  }

  public Task<UsuarioSessao?> GetByJtiAsync(string jti, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(Sessoes.FirstOrDefault(x => x.Jti == jti));
  }

  public Task<UsuarioSessao?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
  {
    return Task.FromResult(Sessoes.FirstOrDefault(x => x.RefreshToken == refreshToken));
  }

  public Task RevokeAllActiveByUsuarioIdAsync(long usuarioId, CancellationToken cancellationToken = default)
  {
    UsuariosRevogados.Add(usuarioId);

    foreach (var sessao in Sessoes.Where(x =>
      x.UsuarioId == usuarioId &&
      !x.Revogado &&
      x.RefreshTokenExpiraEm > DateTime.UtcNow))
    {
      sessao.Revogar();
    }

    return Task.CompletedTask;
  }

  public Task<bool> ExistsActiveSessionAsync(long usuarioId, string jti, CancellationToken cancellationToken = default)
  {
    if (ExistsActiveSessionResultado.HasValue)
      return Task.FromResult(ExistsActiveSessionResultado.Value);

    var agora = DateTime.UtcNow;
    return Task.FromResult(Sessoes.Any(x =>
      x.UsuarioId == usuarioId &&
      x.Jti == jti &&
      !x.Revogado &&
      x.DataRevogacao is null &&
      x.AccessTokenExpiraEm > agora));
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    SaveChangesChamadas++;
    return Task.CompletedTask;
  }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
  public List<string> SenhasHasheadas { get; } = [];
  public List<(string Password, string PasswordHash)> Verificacoes { get; } = [];
  public List<string> RehashChecks { get; } = [];
  public bool VerifyResultado { get; set; } = true;
  public bool NeedsRehashResultado { get; set; }

  public Task<string> HashAsync(string password, CancellationToken cancellationToken = default)
  {
    SenhasHasheadas.Add(password);
    return Task.FromResult($"hashed:{password}");
  }

  public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken = default)
  {
    Verificacoes.Add((password, passwordHash));
    return Task.FromResult(VerifyResultado);
  }

  public bool NeedsRehash(string passwordHash)
  {
    RehashChecks.Add(passwordHash);
    return NeedsRehashResultado;
  }
}

internal sealed class FakeTokenService : ITokenService
{
  public List<Usuario> UsuariosUsados { get; } = [];
  public TokenResult TokenResultado { get; set; } = new(
    "access-token",
    "refresh-token",
    "jti-token",
    DateTime.UtcNow.AddMinutes(15),
    DateTime.UtcNow.AddDays(7));

  public Task<TokenResult> GenerateToken(Usuario usuario)
  {
    UsuariosUsados.Add(usuario);
    return Task.FromResult(TokenResultado);
  }

  public string GenerateRefreshToken() => TokenResultado.RefreshToken;

  public string GetJWT_KEY() => "test-key";

  public string GetJWT_SECRET() => "test-secret";

  public string GetJWT_AUDIENCE() => "test-audience";

  public int GetJWT_ACCESS_TOKEN_MINUTES() => 15;

  public int GetJWT_REFRESH_TOKEN_DAYS() => 7;

  public string GetJWT_ISSUER() => "test-issuer";

  public DateTime GetJWT_ACCESS_TOKEN_EXPIRES() => TokenResultado.AccessTokenExpiraEm;

  public DateTime GetJWT_REFRESH_TOKEN_EXPIRES() => TokenResultado.RefreshTokenExpiraEm;
}

internal sealed class FakeMediator : IMediator
{
  private readonly Dictionary<Type, object?> _responsesByType = [];

  public List<object> Requests { get; } = [];

  public void Respond<TResponse>(TResponse response)
  {
    _responsesByType[typeof(TResponse)] = response;
  }

  public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
  {
    Requests.Add(request);

    if (_responsesByType.TryGetValue(typeof(TResponse), out var response))
      return Task.FromResult((TResponse)response!);

    return Task.FromResult(default(TResponse)!);
  }

  public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
    where TRequest : IRequest
  {
    Requests.Add(request);
    return Task.CompletedTask;
  }

  public Task<object?> Send(object request, CancellationToken cancellationToken = default)
  {
    Requests.Add(request);
    return Task.FromResult<object?>(null);
  }

  public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
    IStreamRequest<TResponse> request,
    CancellationToken cancellationToken = default)
  {
    Requests.Add(request);
    return EmptyStream<TResponse>();
  }

  public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
  {
    Requests.Add(request);
    return EmptyStream<object?>();
  }

  public Task Publish(object notification, CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }

  public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
    where TNotification : INotification
  {
    return Task.CompletedTask;
  }

  private static async IAsyncEnumerable<T> EmptyStream<T>()
  {
    await Task.CompletedTask;
    yield break;
  }
}
