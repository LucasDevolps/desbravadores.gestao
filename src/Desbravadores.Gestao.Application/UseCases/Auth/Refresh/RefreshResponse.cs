using Desbravadores.Gestao.Application.UseCases.Auth.Token;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Refresh;

public sealed record RefreshResponse(TokenResult Token,
    DateTime Expiracao,
    string Nome,
    string Email);

