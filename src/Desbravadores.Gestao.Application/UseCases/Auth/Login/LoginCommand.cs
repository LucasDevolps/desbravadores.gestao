using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Login;

public sealed record LoginCommand(string Email, string Senha) : IRequest<LoginResponse>;
