using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Refresh;

public sealed record RefreshQuery(
    string Sub,
    string Jti
) : IRequest<RefreshResponse>;
