using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Me;

public class MeRequestValidator : AbstractValidator<MeRequest>
{
  public MeRequestValidator()
  {
    RuleFor(x => x.Sub).NotEmpty();
    RuleFor(x => x.Jti).NotEmpty();
  }
}
