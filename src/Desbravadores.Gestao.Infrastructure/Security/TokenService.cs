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
  public Task<TokenResult> GenerateToken(Usuario usuario)
  {
      var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
          ?? throw new InvalidOperationException("JWT_KEY não configurado.");

      var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
          ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");

      var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
          ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");

      var accessTokenMinutes = int.TryParse(Environment.GetEnvironmentVariable("Jwt_ExpiresInMinutes"), out var atm) ? atm : 60;
      var refreshTokenDays = int.TryParse(Environment.GetEnvironmentVariable("Jwt_RefreshTokenDays"), out var rtd) ? rtd : 7;
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var jti = Guid.NewGuid().ToString();
      var accessTokenExpires = DateTime.UtcNow.AddMinutes(accessTokenMinutes);
      var refreshTokenExpires = DateTime.UtcNow.AddDays(refreshTokenDays);

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
          signingCredentials: credentials);

      var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
      var refreshToken = GenerateRefreshToken();

      return Task.FromResult(new TokenResult(accessToken, refreshToken, jti, accessTokenExpires, refreshTokenExpires));
    }

    private static string GenerateRefreshToken()
    {
      var randomBytes = RandomNumberGenerator.GetBytes(64);
      return Convert.ToBase64String(randomBytes);
    }
}
