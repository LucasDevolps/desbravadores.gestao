using FluentValidation;

namespace Desbravadores.Gestao.Application.Auth.Login;

internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
  public LoginCommandValidator()
  {
    RuleFor(x => x.Email.ToLowerInvariant())
    .NotEmpty()
    .EmailAddress()
    .WithMessage("O formato do e-mail é inválido.");

    RuleFor(x => x.Senha)
        .NotEmpty()
        .NotNull()
        .WithMessage("A senha é obrigatória.");
  }
}