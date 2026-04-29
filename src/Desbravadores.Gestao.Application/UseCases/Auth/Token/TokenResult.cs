namespace Desbravadores.Gestao.Application.UseCases.Auth.Token;

public sealed record TokenResult(
    string AccessToken,
    string RefreshToken,
    string Jti,
    DateTime AccessTokenExpiraEm,
    DateTime RefreshTokenExpiraEm);
