using FluentValidation;

namespace Desbravadores.Gestao.Application.Auth.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
  public LoginRequestValidator()
  {
    RuleFor(x => x.Email.ToLowerInvariant())
    .NotEmpty().WithMessage("O e-mail é obrigatório.")
    .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
    .WithMessage("O formato do e-mail é inválido.")
    .EmailAddress().WithMessage("O e-mail informado não é um endereço válido.");

    RuleFor(x => x.Senha)
        .NotEmpty()
        .NotNull()
        .WithMessage("A senha é obrigatória.");
  }
}