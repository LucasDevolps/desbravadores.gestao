using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Refresh;

public class RefreshQueryHandler(
  ITokenService tokenService,
  IUsuarioRepository usuarioRepository,
  IUsuarioSessaoRepository usuarioSessaoRepository
)
: IRequestHandler<RefreshQuery, RefreshResponse>
{
  private readonly ITokenService _tokenService = tokenService;
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;

  public async Task<RefreshResponse> Handle(RefreshQuery request, CancellationToken cancellationToken)
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
    
    await _usuarioSessaoRepository.RevokeAllActiveByUsuarioIdAsync(usuario.Id, cancellationToken);

    TokenResult tokenRefresh = await _tokenService.GenerateToken(usuario);

    UsuarioSessao sessao = new UsuarioSessao(
      usuario.Id,
      request.Jti,
      tokenRefresh.RefreshToken,
      _tokenService.GetJWT_ACCESS_TOKEN_EXPIRES(),
      _tokenService.GetJWT_REFRESH_TOKEN_EXPIRES()
    );

    await _usuarioSessaoRepository.AddAsync(sessao, cancellationToken);
    await _usuarioSessaoRepository.SaveChangesAsync(cancellationToken);

    return new RefreshResponse
    (
      tokenRefresh,
      _tokenService.GetJWT_ACCESS_TOKEN_EXPIRES(),
      usuario.Nome,
      usuario.Email
    );
  }
}
