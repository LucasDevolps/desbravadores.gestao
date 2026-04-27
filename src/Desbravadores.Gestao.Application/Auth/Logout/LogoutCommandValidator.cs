using FluentValidation;

namespace Desbravadores.Gestao.Application.Auth.Logout;

internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
  public LogoutCommandValidator() {
    RuleFor(x => x.Jti).NotEmpty();
  } 
}
