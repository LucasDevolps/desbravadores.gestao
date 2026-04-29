using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Logout;

public sealed record LogoutCommand(string Jti) : IRequest<Unit>;
