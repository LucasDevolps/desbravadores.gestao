
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioRequestHandler(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;

  public async Task<Guid> HandleAsync(CriarUsuarioRequest command, CancellationToken cancellationToken = default)
  {
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(command.Email, cancellationToken);

    if (usuarioExistente is not null)
      throw new InvalidOperationException("Já existe um usuário cadastrado com o email informado.");

    var senhaHash = await _passwordHasher.HashAsync(command.Senha, cancellationToken);

    var usuario = new Usuario(
      command.Nome.Trim(),
      command.Email.Trim().ToLowerInvariant(),
      senhaHash,
      command.Roles.Trim()
    );

    await _usuarioRepository.AdicionarUsuarioAsync(usuario, cancellationToken);
    return usuario.Uuid;
  }
}
