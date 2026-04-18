
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioCommandHandler(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher)
        : IRequestHandler<CriarUsuarioCommand, Guid>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;

  public async Task<Guid> Handle(
      CriarUsuarioCommand request,
      CancellationToken cancellationToken)
  {
    var usuarioExistente = await _usuarioRepository.GetByEmailAsync(
        request.Email,
        cancellationToken);

    if (usuarioExistente is not null)
      throw new InvalidOperationException("Já existe um usuário com este e-mail.");

    var senhaHash = await _passwordHasher.HashAsync(request.Senha);

    var usuario = new Usuario(
        request.Nome,
        request.Email,
        senhaHash,
        Enum.TryParse<Roles>(request.Roles, true,out var role) ? role : Roles.DESBRAVADOR);

    await _usuarioRepository.AdicionarUsuarioAsync(usuario, cancellationToken);

    return usuario.Uuid;
  }
}