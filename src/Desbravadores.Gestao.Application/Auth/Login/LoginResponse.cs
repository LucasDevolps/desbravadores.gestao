using Desbravadores.Gestao.Application.Auth.Token;

namespace Desbravadores.Gestao.Application.Auth.Login;

public sealed record LoginResponse(
    TokenResult  Token,
    DateTime Expiracao,
    string Nome,
    string Email
);