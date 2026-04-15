using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Logout;

public sealed record LogoutCommand(string Jti) : IRequest<Unit>;
