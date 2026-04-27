using FluentValidation;

namespace Desbravadores.Gestao.Application.Auth.Refresh;

internal class RefreshQueryValidator : AbstractValidator<RefreshQuery>
{
  public RefreshQueryValidator()
  {
    RuleFor(x => x.Sub)
    .NotEmpty()
    .WithMessage("O campo 'Sub' é obrigatório.");

    RuleFor(x => x.Jti)
      .NotEmpty()
      .WithMessage("O campo 'Jti' é obrigatório.");
  }
}
