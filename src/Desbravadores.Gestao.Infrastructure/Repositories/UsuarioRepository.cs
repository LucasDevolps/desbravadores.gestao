using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Domain;
using Desbravadores.Gestao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository
{
  private readonly AppDbContext _context = context;

  public async Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
    => await _context.Usuarios.AddAsync(usuario, cancellationToken);

  public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
    => await _context.Usuarios.ToListAsync(cancellationToken);
  public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await _context
        .Usuarios
        .FirstOrDefaultAsync(x => x.Email.Equals(email.Trim().ToLowerInvariant()), cancellationToken);
  }

  public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Usuarios.FindAsync(id, cancellationToken);

  public async Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default)
    => await _context.Usuarios.Where(u => u.Id == usuario.Id).ExecuteDeleteAsync(cancellationToken);

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    => await _context.SaveChangesAsync(cancellationToken);

  public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    => throw new NotImplementedException("Ainda não existe update");
}
