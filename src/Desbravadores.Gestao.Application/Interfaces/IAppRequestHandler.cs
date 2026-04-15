namespace Desbravadores.Gestao.Application.Interfaces;

public interface IAppRequestHandler<in TRequest, TResponse>
{
  Task<TResponse> HandleAsync(
      TRequest request,
      CancellationToken cancellationToken = default);
}