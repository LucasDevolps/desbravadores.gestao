using MediatR;

namespace Desbravadores.Gestao.Application.Common;

public sealed record EmptyRequest : IRequest<Guid>;
