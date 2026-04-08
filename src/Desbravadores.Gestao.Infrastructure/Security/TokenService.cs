using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Desbravadores.Gestao.Infrastructure.Security;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
  private readonly IConfiguration _configuration = configuration;

  public Task<TokenResult> GenerateToken(Usuario usuario)
  {
    var key = _configuration["Jwt:Key"]
              ?? throw new InvalidOperationException("Jwt:Key não configurado.");

    var issuer = _configuration["Jwt:Issuer"]
                 ?? throw new InvalidOperationException("Jwt:Issuer não configurado.");

    var audience = _configuration["Jwt:Audience"]
                   ?? throw new InvalidOperationException("Jwt:Audience não configurado.");

    var expiresInMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var minutes)
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
