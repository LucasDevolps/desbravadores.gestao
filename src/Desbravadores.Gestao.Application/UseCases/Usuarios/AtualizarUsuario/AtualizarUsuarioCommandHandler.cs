using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed class AtualizarUsuarioCommandHandler
(IUsuarioRepository usuarioRepository) 
: IRequestHandler<AtualizarUsuarioCommand, UsuarioDTO>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  public async Task<UsuarioDTO> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
  {
    var usuario = await _usuarioRepository.GetByIdAsync(request.Command.Uuid, cancellationToken);

    if (usuario is null)
      throw new KeyNotFoundException("Usuario não encontrado");

    await _usuarioRepository.UpdateAsync(request.Command, cancellationToken); 
    
    return usuario;
  }
}
