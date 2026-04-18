using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Me;

public sealed class MeQueryHandler(
  IUsuarioSessaoRepository usuarioSessaoRepository,
  IUsuarioRepository usuarioRepository)
  :IRequestHandler<MeQuery, UsuarioDTO>
{
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

  public async Task<UsuarioDTO> Handle(MeQuery request, CancellationToken cancellationToken = default)
  {
    var uuidValue = request.Sub;

    if (!Guid.TryParse(uuidValue, out var uuid))
      throw new UnauthorizedAccessException("Token inválido: UUID não encontrado.");

    if (string.IsNullOrWhiteSpace(request.Jti))
      throw new UnauthorizedAccessException("Token inválido: JTI não encontrado.");

    var usuario = await _usuarioRepository.GetByUuidAsync(uuid, cancellationToken) 
      ?? throw new KeyNotFoundException("Usuário não encontrado.");
    
      var tokenValido = await _usuarioSessaoRepository.ExistsActiveSessionAsync(
        usuario.Id,
        request.Jti,
        cancellationToken);

    if (!tokenValido)
      throw new UnauthorizedAccessException("Token revogado, expirado ou inválido.");

    return new UsuarioDTO().FromEntity(usuario);
  }
}