using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUsuarioRepository usuarioRepository,
    IUsuarioSessaoRepository usuarioSessaoRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, LoginResponse>
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly IUsuarioSessaoRepository _usuarioSessaoRepository = usuarioSessaoRepository;
  private readonly ITokenService _tokenService = tokenService;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;

  public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken = default)
  {
    var usuario = await _usuarioRepository.GetByEmailAsync(request.Email, cancellationToken)
      ?? throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

    var senhaValida = await _passwordHasher.VerifyAsync(request.Senha, usuario.Senha, cancellationToken);

    if (!senhaValida)
      throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

    await _usuarioSessaoRepository.RevokeAllActiveByUsuarioIdAsync(usuario.Id, cancellationToken);

    TokenResult token = await _tokenService.GenerateToken(usuario);

    var sessao = new UsuarioSessao(
      usuario.Id,
      token.Jti,
      token.RefreshToken,
      token.AccessTokenExpiraEm,
      token.RefreshTokenExpiraEm);

    await _usuarioSessaoRepository.AddAsync(sessao, cancellationToken);
    await _usuarioSessaoRepository.SaveChangesAsync(cancellationToken);

    return new LoginResponse(
      token,
      token.AccessTokenExpiraEm,
      usuario.Nome,
      usuario.Email);
  }
}
