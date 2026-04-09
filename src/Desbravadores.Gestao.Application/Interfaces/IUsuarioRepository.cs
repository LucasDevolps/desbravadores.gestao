using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UsuarioDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<IEnumerable<UsuarioDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
    
}
