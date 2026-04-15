namespace Desbravadores.Gestao.Api.Interfaces;

public interface IRequestHandler<in TRequest, TResponse>
{
  Task<TResponse> HandleAsync(
      TRequest request,
      CancellationToken cancellationToken = default);
}
