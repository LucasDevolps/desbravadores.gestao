using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Login;

public sealed record LoginRequestQuery(string Email, string Senha) : IRequest<LoginResponse>;
