using Desbravadores.Gestao.Application.Auth.Token;
using Desbravadores.Gestao.Domain;

namespace Desbravadores.Gestao.Application.Interfaces;

public interface ITokenService
{
  Task<TokenResult> GenerateToken(Usuario usuario);
}
