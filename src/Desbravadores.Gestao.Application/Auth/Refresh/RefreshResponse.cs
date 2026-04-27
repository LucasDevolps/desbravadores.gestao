using Desbravadores.Gestao.Application.Auth.Token;

namespace Desbravadores.Gestao.Application.Auth.Refresh;

public sealed record RefreshResponse(TokenResult Token,
    DateTime Expiracao,
    string Nome,
    string Email);

