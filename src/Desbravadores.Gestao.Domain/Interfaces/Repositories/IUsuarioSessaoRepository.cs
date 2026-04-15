using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Domain.Interfaces.Repositories;

public interface IUsuarioSessaoRepository
{
  Task AddAsync(UsuarioSessao sessao, CancellationToken cancellationToken = default);
  Task<UsuarioSessao?> GetByJtiAsync(string jti, CancellationToken cancellationToken = default);
  Task<UsuarioSessao?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
<<<<<<< HEAD
  Task RevokeAllActiveByUsuarioIdAsync(long usuarioId, CancellationToken cancellationToken = default);
  Task<bool> ExistsActiveSessionAsync(long usuarioId, string jti, CancellationToken cancellationToken = default);
=======
>>>>>>> parent of 7a4b8d1 (Revoga sessões ativas do usuário ao realizar login)
  Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
