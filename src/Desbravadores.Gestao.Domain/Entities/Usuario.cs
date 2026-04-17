namespace Desbravadores.Gestao.Domain.Entities;

public sealed class Usuario
{
  public Guid Uuid { get; private set; } = Guid.NewGuid();
  public int Id { get; private set; }
  public string Nome { get; private set; }
  public string Email { get; private set; }
  public string Senha { get; private set; }
  public DateOnly DataCriacao { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
  public string Role { get; private set; }

  public ICollection<UsuarioSessao> Sessoes { get; private set; } = new List<UsuarioSessao>();

  // Construtor principal
  public Usuario(Guid id, string nome, string email, string senha, string role)
  {
    Uuid = id;
    Nome = nome;
    Email = email;
    Senha = senha;
    Role = role;
  }

  // Construtor para EF Core
  private Usuario() { }
}