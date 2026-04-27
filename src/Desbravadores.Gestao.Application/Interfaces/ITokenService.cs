using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Domain.Entities;
using System.Security.Cryptography;

namespace Desbravadores.Gestao.Application.Interfaces;

public interface ITokenService
{
  Task<TokenResult> GenerateToken(Usuario usuario);
  string GenerateRefreshToken();
  string GetJWT_KEY();

  string GetJWT_SECRET();

  string GetJWT_AUDIENCE();
  int GetJWT_ACCESS_TOKEN_MINUTES();

  int GetJWT_REFRESH_TOKEN_DAYS();

  string GetJWT_ISSUER();

  DateTime GetJWT_ACCESS_TOKEN_EXPIRES();
  DateTime GetJWT_REFRESH_TOKEN_EXPIRES();

}
