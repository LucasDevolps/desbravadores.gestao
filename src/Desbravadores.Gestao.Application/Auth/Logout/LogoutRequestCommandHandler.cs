using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Logout;

public sealed class LogoutRequestCommandHandler(IUsuarioSessaoRepository usuarioSessaoRepository) :IRequestHandler<LogoutCommand, Unit>
{
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;
  public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(request.Jti))
      throw new UnauthorizedAccessException("Token de acesso inválido.");

    var sessao = await _usuarioSessaoRepository.GetByJtiAsync(request.Jti, cancellationToken) 
      ?? throw new UnauthorizedAccessException("Sessão não encontrada.");

    sessao.Revogar();

    await _usuarioSessaoRepository.SaveChangesAsync(cancellationToken);

    return Unit.Value;
  }
}
