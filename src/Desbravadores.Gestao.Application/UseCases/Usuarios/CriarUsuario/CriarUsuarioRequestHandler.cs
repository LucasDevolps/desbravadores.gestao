
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioRequestHandler(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher)
    : IAppRequestHandler<CriarUsuarioRequest, Guid>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;

  public async Task<Guid> HandleAsync(
      CriarUsuarioRequest request,
      CancellationToken cancellationToken = default)
  {
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(
        request.Email,
        cancellationToken);

    if (usuarioExistente is not null)
      throw new InvalidOperationException("Já existe um usuário cadastrado com o e-mail informado.");

    var senhaHash = await _passwordHasher.HashAsync(
        request.Senha,
        cancellationToken);

    var usuario = new Usuario(
        request.Nome.Trim(),
        request.Email.Trim().ToLowerInvariant(),
        senhaHash,
        request.Roles.Trim());

    await _usuarioRepository.AdicionarUsuarioAsync(
        usuario,
        cancellationToken);

    return usuario.Uuid;
  }
}