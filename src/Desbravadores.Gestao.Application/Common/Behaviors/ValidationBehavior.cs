using FluentValidation;
using MediatR;

namespace Desbravadores.Gestao.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
  IEnumerable<IValidator<TRequest>> validators)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : notnull
{
  private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    if (!_validators.Any())
      return await next(cancellationToken);

    var context = new ValidationContext<TRequest>(request);

    var validationResults = await Task.WhenAll(
      _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

    var failures = validationResults
      .SelectMany(result => result.Errors)
      .Where(failure => failure is not null)
      .ToList();

    if (failures.Count != 0)
      throw new ValidationException(failures);

    return await next(cancellationToken);
  }
}
