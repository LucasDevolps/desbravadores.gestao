using FluentValidation;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;

internal sealed class DeletarusuarioCommandValidator : AbstractValidator<DeletarUsuarioCommand>
{
  public DeletarusuarioCommandValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty().WithMessage("O Id do usuário é obrigatório.")
        .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("O Id do usuário deve ser um GUID válido.");
  }
}
