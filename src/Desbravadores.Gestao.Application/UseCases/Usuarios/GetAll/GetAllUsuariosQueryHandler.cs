using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;

public class GetAllUsuariosQueryHandler(IUsuarioRepository usuarioRepository) : IRequestHandler<GetAllUsuariosQuery, IEnumerable<UsuarioDTO>>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

  public async Task<IEnumerable<UsuarioDTO>> Handle(
        GetAllUsuariosQuery query,
        CancellationToken cancellationToken = default)
  {
    return await _usuarioRepository.GetAllAsync(cancellationToken);
  }
}
