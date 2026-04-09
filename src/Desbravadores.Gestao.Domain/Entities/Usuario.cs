using Desbravadores.Gestao.Domain.Constants;

namespace Desbravadores.Gestao.Domain.Entities;

public sealed class Usuario(string nome, string email, string senha, string role)
{
  public Guid Uuid { get; private set; } = Guid.NewGuid();
  public int Id { get; private set; } = default;
  public string Nome { get; private set; } = nome;
  public string Email { get; private set; } = email;
  public string Senha { get; private set; } = senha;
  public DateOnly DataCriacao { get; private set; } = DateOnly.MinValue;

  public string Role { get; private set; } = role;

  public ICollection<UsuarioSessao> Sessoes { get; private set; } = [];

}
