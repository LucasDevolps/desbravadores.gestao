using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed class AtualizarUsuarioCommandHandler
(IUsuarioRepository usuarioRepository) 
: IRequestHandler<AtualizarUsuarioCommand, UsuarioDTO>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  public async Task<UsuarioDTO> Handle(AtualizarUsuarioCommand request, CancellationToken cancellationToken)
  {
    var usuario = await _usuarioRepository.GetByIdAsync(request.command.Uuid, cancellationToken);

    if (usuario is null)
      throw new KeyNotFoundException("Usuario não encontrado");

    await _usuarioRepository.UpdateAsync(request.command, cancellationToken); 
    
    return usuario;
  }
}
