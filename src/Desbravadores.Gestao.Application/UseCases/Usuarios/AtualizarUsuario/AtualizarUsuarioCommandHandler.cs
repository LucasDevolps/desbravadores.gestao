using Desbravadores.Gestao.Application.DTOs;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed class AtualizarUsuarioCommandHandler(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<AtualizarUsuarioCommand, UsuarioDTO>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;

  public async Task<UsuarioDTO> Handle(
      AtualizarUsuarioCommand request,
      CancellationToken cancellationToken)
  {
    if (request.UsuarioLogado is null || request.UsuarioLogado == Guid.Empty)
      throw new UnauthorizedAccessException("Usuário logado inválido.");

    if (string.IsNullOrWhiteSpace(request.IpUsuarioLogado))
      throw new InvalidOperationException("IP do usuário logado é obrigatório.");

    var usuarioLogado = await _usuarioRepository.GetByUuidAsync(request.UsuarioLogado.Value, cancellationToken)
        ?? throw new UnauthorizedAccessException("Usuário logado não encontrado.");

    var usuario = await _usuarioRepository.GetByUuidAsync(request.Uuid, cancellationToken)
        ?? throw new KeyNotFoundException("Usuário não encontrado.");

    if (!string.IsNullOrWhiteSpace(request.Nome))
      usuario.AtualizarNome(request.Nome);

    if (!string.IsNullOrWhiteSpace(request.Email))
    {
      var emailNormalizado = request.Email.Trim().ToLowerInvariant();

      var usuarioComMesmoEmail = await _usuarioRepository.GetByEmailAsync(
          emailNormalizado,
          cancellationToken);

      if (usuarioComMesmoEmail is not null && usuarioComMesmoEmail.Uuid != usuario.Uuid)
        throw new InvalidOperationException("Já existe um usuário com este e-mail.");

      usuario.AtualizarEmail(emailNormalizado);
    }

    if (!string.IsNullOrWhiteSpace(request.Senha))
    {
      var senhaHash = await _passwordHasher.HashAsync(request.Senha, cancellationToken);
      usuario.AtualizarSenha(senhaHash);
    }

    if (!string.IsNullOrWhiteSpace(request.Roles))
    {
      if (!Enum.TryParse<Roles>(request.Roles, true, out var role))
        throw new InvalidOperationException("Role inválida.");

      usuario.AtualizarRole(role);
    }

    usuario.RegistrarAtualizacao(usuarioLogado, request.IpUsuarioLogado);

    await _usuarioRepository.SaveChangesAsync(cancellationToken);

    return new UsuarioDTO().FromEntity(usuario);
  }
}
