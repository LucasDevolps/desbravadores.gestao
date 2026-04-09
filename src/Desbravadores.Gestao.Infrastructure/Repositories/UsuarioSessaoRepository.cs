using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using Desbravadores.Gestao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Repositories;

public sealed class UsuarioSessaoRepository(AppDbContext context) : IUsuarioSessaoRepository
{
  private readonly AppDbContext _context = context;

  public async Task AddAsync(UsuarioSessao sessao, CancellationToken cancellationToken = default)
  {
    await _context.UsuarioSessoes.AddAsync(sessao, cancellationToken);
  }

  public async Task<UsuarioSessao?> GetByJtiAsync(string jti, CancellationToken cancellationToken = default)
  {
    return await _context.UsuarioSessoes
        .FirstOrDefaultAsync(x => x.Jti == jti, cancellationToken);
  }

  public async Task<UsuarioSessao?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
  {
    return await _context.UsuarioSessoes
        .Include(x => x.Usuario)
        .FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }
}
