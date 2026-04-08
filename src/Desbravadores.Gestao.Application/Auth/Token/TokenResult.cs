namespace Desbravadores.Gestao.Application.Auth.Token;

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);
