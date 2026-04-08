using Desbravadores.Gestao.Domain;

namespace Desbravadores.Gestao.Application;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
    
}
