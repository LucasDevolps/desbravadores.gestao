namespace Desbravadores.Gestao.Domain.DTOs;

public sealed class UsuarioDTO()
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Nome { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public DateOnly DataCriacao { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

}
