using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;

namespace Desbravadores.Gestao.Application.Auth.Login;

public sealed class LoginHandler(
    IUsuarioRepository usuarioRepository,
    ITokenService tokenService)
{
  private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
  private readonly ITokenService _tokenService = tokenService;

  public async Task<LoginResponse> HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
  {
    var usuario = await _usuarioRepository.GetByEmailAsync(request.Email, cancellationToken) 
    ?? throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

    TokenResult token = await _tokenService.GenerateToken(usuario);
    var expiracao = DateTime.UtcNow.AddMinutes(await GetExpirateTimeMinutes());

    return new LoginResponse(
        Token: token,
        Expiracao: expiracao,
        Nome: usuario.Nome,
        Email: usuario.Email
    );
  }
  private static async Task<int> GetExpirateTimeMinutes()
  {
    var jwtExpiration = Environment.GetEnvironmentVariable("Jwt_ExpiresInMinutes");
    if (int.TryParse(jwtExpiration, out int expirationMinutes))
    {
      return await Task.FromResult(expirationMinutes);
    }
    throw new InvalidOperationException("Jwt_ExpiresInMinutes não configurado ou inválido.");
  }
}
