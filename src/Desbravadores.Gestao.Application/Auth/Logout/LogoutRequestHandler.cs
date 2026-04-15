using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Logout;

public sealed class LogoutRequestHandler(IUsuarioSessaoRepository usuarioSessaoRepository) :IAppRequestHandler<string, Unit>
{
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;
  public async Task<Unit> HandleAsync(string jti, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(jti))
      throw new UnauthorizedAccessException("Token de acesso inválido.");

    var sessao = await _usuarioSessaoRepository.GetByJtiAsync(jti, cancellationToken) 
      ?? throw new UnauthorizedAccessException("Sessão não encontrada.");

    sessao.Revogar();

    await _usuarioSessaoRepository.SaveChangesAsync(cancellationToken);

    return Unit.Value;
  }
}
