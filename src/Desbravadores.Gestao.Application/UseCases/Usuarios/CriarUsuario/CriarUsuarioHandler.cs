using Desbravadores.Gestao.Domain;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioHandler(IUsuarioRepository usuarioRepository)
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

  public async Task<Guid> HandleAsync(CriarUsuario command, CancellationToken cancellationToken = default)
  {
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(command.Email, cancellationToken);

    if (usuarioExistente is not null)
      throw new InvalidOperationException("Já existe um usuário cadastrado com o email informado.");

    var senhaHash = BCrypt.Net.BCrypt.HashPassword(command.Senha);

    var usuario = new Usuario(
      command.Nome.Trim(),
      command.Email.Trim().ToLowerInvariant(),
      senhaHash
    );

    await _usuarioRepository.AdicionarUsuarioAsync(usuario, cancellationToken);
    await _usuarioRepository.SaveChangesAsync(cancellationToken);

    return usuario.Id;
  }
}
