using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;

public class BuscaPorIdRequestHandler(IUsuarioRepository usuarioRepository) : IAppRequestHandler<Guid, UsuarioDTO>
{
  private readonly IUsuarioRepository usuarioRepository = usuarioRepository;
  public async Task<UsuarioDTO> HandleAsync(Guid request, CancellationToken cancellationToken = default)
  {
    return await usuarioRepository.GetByIdAsync(request, cancellationToken) 
      ?? throw new KeyNotFoundException("Usuário não encontrado.");
  }
}
