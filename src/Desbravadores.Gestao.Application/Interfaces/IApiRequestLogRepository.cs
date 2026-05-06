using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Application.Interfaces;

public interface IApiRequestLogRepository
{
  Task AddAsync(ApiRequestLog apiRequestLog, CancellationToken cancellationToken = default);
}
