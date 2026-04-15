using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Me;

public class MeQueryValidator : AbstractValidator<MeQuery>
{
  public MeQueryValidator()
  {
    RuleFor(x => x.Sub).NotEmpty();
    RuleFor(x => x.Jti).NotEmpty();
  }
}
