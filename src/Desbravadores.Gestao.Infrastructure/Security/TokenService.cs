using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Desbravadores.Gestao.Infrastructure.Security;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
  public Task<TokenResult> GenerateToken(Usuario usuario)
  {
    var key = Environment.GetEnvironmentVariable("JWT_KEY")
              ?? throw new InvalidOperationException("JWT_KEY não configurado.");

    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                 ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");

    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                   ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");

    var expiresInMinutes = int.TryParse(Environment.GetEnvironmentVariable("Jwt_ExpiresInMinutes"), out var minutes)
        ? minutes
        : 120;

    var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new("uuid", usuario.Id.ToString())
        };

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
        signingCredentials: credentials
    );

    return Task.FromResult(new TokenResult(
        AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
        ExpiresAtUtc: token.ValidTo
    ));
  }
}
