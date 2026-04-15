using Desbravadores.Gestao.Application.Common;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : IUsuarioRepository 
{
  private readonly AppDbContext _context = context;

  public async Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
  {
    await _context.Usuarios.AddAsync(usuario, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<IEnumerable<UsuarioDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    => await _context.Usuarios
        .Select(u => new UsuarioDTO
        {
            Id = u.Uuid,
            Nome = u.Nome,
            Email = u.Email,
            DataCriacao = u.DataCriacao
        })
        .ToListAsync(cancellationToken);

  public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await _context
        .Usuarios
        .FirstOrDefaultAsync(x => x.Email.Equals(email.Trim().ToLowerInvariant()), cancellationToken);
  }

  public async Task<UsuarioDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    => await _context.Usuarios
        .Where(u => u.Uuid == id)
        .Select(u => new UsuarioDTO
        {
            Id = u.Uuid,
            Nome = u.Nome,
            Email = u.Email,
            DataCriacao = u.DataCriacao
        })
        .FirstOrDefaultAsync(cancellationToken);

  public async Task<Usuario?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
  {
    return await _context
        .Usuarios
        .FirstOrDefaultAsync(x => x.Uuid == uuid, cancellationToken);
  }
  public async Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default)
  {
      await _context.Usuarios.Where(u => u.Uuid == usuario.Uuid).ExecuteDeleteAsync(cancellationToken);
      await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default)
  {
    _context.Usuarios.Update(usuario);
    await _context.SaveChangesAsync(cancellationToken);
  }
}
