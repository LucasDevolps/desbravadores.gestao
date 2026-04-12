using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed class CriarUsuarioValidation : AbstractValidator<CriarUsuarioRequest>
{
  public CriarUsuarioValidation()
  {
    RuleFor(x => x.Nome)
        .NotEmpty()
        .WithMessage("O nome é obrigatório.");
    RuleFor(x => x.Email.ToLowerInvariant())
        .NotEmpty().WithMessage("O e-mail é obrigatório.")
        .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
        .WithMessage("O formato do e-mail é inválido.")
        .EmailAddress().WithMessage("O e-mail informado não é um endereço válido.");
    RuleFor(x => x.Senha)
        .NotEmpty()
        .NotNull()
        .WithMessage("A senha é obrigatória.");
    RuleFor(x => x.Roles)
        .NotEmpty()
        .IsInEnum()
        .WithMessage("A role é obrigatória e precisa ser um campo válido.");

  }
}
