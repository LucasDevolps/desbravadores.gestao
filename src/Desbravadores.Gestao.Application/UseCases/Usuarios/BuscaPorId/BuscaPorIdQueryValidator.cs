using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;

internal sealed class BuscaPorIdQueryValidator : AbstractValidator<BuscaUsuarioPorIdQuery>
{
  public BuscaPorIdQueryValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty().WithMessage("O Id do usuário é obrigatório.")
      .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("O Id do usuário deve ser um GUID válido.");
  }
}
