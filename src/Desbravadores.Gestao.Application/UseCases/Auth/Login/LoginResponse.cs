using Desbravadores.Gestao.Application.UseCases.Auth.Token;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Login;

public sealed record LoginResponse(
    TokenResult Token,
    DateTime Expiracao,
    string Nome,
    string Email
);
