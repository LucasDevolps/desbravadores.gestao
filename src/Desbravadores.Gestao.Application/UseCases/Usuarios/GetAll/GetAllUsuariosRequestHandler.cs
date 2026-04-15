using Desbravadores.Gestao.Application.Common;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;

public class GetAllUsuariosRequestHandler(IUsuarioRepository usuarioRepository) : IAppRequestHandler<EmptyRequest, IEnumerable<UsuarioDTO>>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

  public async Task<IEnumerable<UsuarioDTO>> HandleAsync(
        EmptyRequest request,
        CancellationToken cancellationToken = default)
  {
    return await _usuarioRepository.GetAllAsync(cancellationToken);
  }
}
