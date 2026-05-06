using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Infrastructure.Data;

namespace Desbravadores.Gestao.Infrastructure.Repositories;

public sealed class ApiRequestLogRepository(AppDbContext context) : IApiRequestLogRepository
{
  private readonly AppDbContext _context = context;

  public async Task AddAsync(ApiRequestLog apiRequestLog, CancellationToken cancellationToken = default)
  {
    await _context.ApiRequestLogs.AddAsync(apiRequestLog, cancellationToken);
    await _context.SaveChangesAsync(cancellationToken);
  }
}
