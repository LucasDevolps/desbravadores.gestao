using Desbravadores.Gestao.Domain.Interfaces.Repositories;

namespace Desbravadores.Gestao.Application.Auth.Logout;

public sealed class LogoutRequestHandler(IUsuarioSessaoRepository usuarioSessaoRepository)
{
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;
  public async Task HandleAsync(string jti, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(jti))
      throw new UnauthorizedAccessException("Token de acesso inválido.");

    var sessao = await usuarioSessaoRepository.GetByJtiAsync(jti, cancellationToken);

    if (sessao is null)
      throw new UnauthorizedAccessException("Sessão não encontrada.");

    sessao.Revogar();
    await usuarioSessaoRepository.SaveChangesAsync(cancellationToken);
  }
}
