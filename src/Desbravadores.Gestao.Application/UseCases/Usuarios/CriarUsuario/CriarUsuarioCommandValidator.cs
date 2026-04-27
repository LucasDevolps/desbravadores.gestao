using Desbravadores.Gestao.Domain.Constants;
using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

internal sealed class CriarUsuarioCommandValidator : AbstractValidator<CriarUsuarioCommand>
{
  public CriarUsuarioCommandValidator()
  {
    RuleFor(x => x.Nome)
      .NotEmpty()
      .WithMessage("O nome é obrigatório.");

    RuleFor(x => x.Email.ToLowerInvariant())
      .NotEmpty()
      .EmailAddress()
      .WithMessage("O e-mail informado não é um endereço válido.");

    RuleFor(x => x.Senha)
      .NotEmpty()
      .NotNull()
      .WithMessage("A senha é obrigatória.");

    RuleFor(x => x.Roles)
      .NotEmpty()
      .IsInEnum()
      .Must(role => Enum.TryParse(typeof(Roles), role, true, out _))
      .WithMessage("Role inválida.");
  }
}
