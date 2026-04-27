using MediatR;

namespace Desbravadores.Gestao.Application.Auth.Refresh;

public sealed record RefreshQuery(
    string Sub,
    string Jti
) : IRequest<RefreshResponse>;
