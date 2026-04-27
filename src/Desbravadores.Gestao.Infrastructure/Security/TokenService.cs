using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Desbravadores.Gestao.Infrastructure.Security;

public sealed class TokenService : ITokenService
{
  public async Task<TokenResult> GenerateToken(Usuario usuario)
  {
    var jwtKey = GetJWT_KEY();

    var issuer = GetJWT_ISSUER();

    var audience = GetJWT_AUDIENCE();

    var accessTokenMinutes = GetJWT_ACCESS_TOKEN_MINUTES();

    var refreshTokenDays = GetJWT_REFRESH_TOKEN_DAYS();

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var jti = Guid.NewGuid().ToString();

    var accessTokenExpires = GetJWT_ACCESS_TOKEN_EXPIRES();

    var refreshTokenExpires = GetJWT_REFRESH_TOKEN_EXPIRES();

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, usuario.Uuid.ToString()),
      new(JwtRegisteredClaimNames.Email, usuario.Email),
      new(JwtRegisteredClaimNames.Jti, jti),
      new("user_id", usuario.Id.ToString()),
      new(ClaimTypes.Name, usuario.Nome),
      new(ClaimTypes.Role, usuario.Role.ToString())
    };

    var token = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: claims,
          expires: accessTokenExpires,
          signingCredentials: credentials
    );

    var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
    var refreshToken = GenerateRefreshToken();

    return await Task.FromResult(new TokenResult(accessToken, refreshToken, jti, accessTokenExpires, refreshTokenExpires));
  }

  public string GenerateRefreshToken()
  {
    var randomBytes = RandomNumberGenerator.GetBytes(64);
    return Convert.ToBase64String(randomBytes);
  }
  public string GetJWT_KEY()
  {
    return Environment.GetEnvironmentVariable("JWT_KEY")
        ?? throw new InvalidOperationException("JWT_KEY não configurado.");
  }
  public string GetJWT_SECRET()
  {
    return Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET não configurado.");
  }
  public string GetJWT_AUDIENCE()
  {
    return Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");
  }
  public int GetJWT_ACCESS_TOKEN_MINUTES()
  {
    return int
            .TryParse(
              Environment
              .GetEnvironmentVariable("Jwt_ExpiresInMinutes")
              , out var atm
            ) ? atm : 15;
  }
  public int GetJWT_REFRESH_TOKEN_DAYS()
  {
    return int
            .TryParse(
              Environment
              .GetEnvironmentVariable("Jwt_RefreshTokenExpiresInDays")
              , out var rtd
            ) ? rtd : 7;
  }
  public string GetJWT_ISSUER()
  {
    return Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");
  }
  public DateTime GetJWT_ACCESS_TOKEN_EXPIRES()
  {
    var accessTokenMinutes = GetJWT_ACCESS_TOKEN_MINUTES();
    return DateTime.UtcNow.AddMinutes(accessTokenMinutes);
  }
  public DateTime GetJWT_REFRESH_TOKEN_EXPIRES()
  {
    var refreshTokenDays = GetJWT_REFRESH_TOKEN_DAYS();
    return DateTime.UtcNow.AddDays(refreshTokenDays);
  }
}
