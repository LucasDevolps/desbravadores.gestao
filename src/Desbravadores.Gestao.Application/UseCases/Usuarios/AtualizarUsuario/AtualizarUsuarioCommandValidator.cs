using FluentValidation;

using Desbravadores.Gestao.Domain.Constants;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

internal sealed class AtualizarUsuarioCommandValidator : AbstractValidator<AtualizarUsuarioCommand>
{
  public AtualizarUsuarioCommandValidator()
  {
    RuleFor(x => x.Uuid)
        .NotEmpty().WithMessage("O Id do usuário é obrigatório.");

    RuleFor(x => x.Nome)
        .NotEmpty().WithMessage("O nome do usuário é obrigatório.")
        .MaximumLength(100).WithMessage("O nome do usuário deve ter no máximo 100 caracteres.");

    RuleFor(x => x.Email)
        .NotEmpty().WithMessage("O email do usuário é obrigatório.")
        .EmailAddress()
        .WithMessage("O email do usuário deve ser um endereço de email válido.")
        .MaximumLength(255).WithMessage("O email do usuário deve ter no máximo 255 caracteres.");

    RuleFor(x => x.Senha)
        .MinimumLength(6).WithMessage("A senha do usuário deve ter no mínimo 6 caracteres.")
        .MaximumLength(100).WithMessage("A senha do usuário deve ter no máximo 100 caracteres.");

    RuleFor(x => x.Roles)
        .Must(role => string.IsNullOrWhiteSpace(role) || Enum.TryParse<Roles>(role, true, out _))
        .WithMessage("Role inválida.");
  }
}
