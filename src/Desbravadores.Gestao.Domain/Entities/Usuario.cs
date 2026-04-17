using Desbravadores.Gestao.Domain.Constants;
using System.Data;

namespace Desbravadores.Gestao.Domain.Entities;

public sealed class Usuario
{
  public Guid Uuid { get; private set; } = Guid.NewGuid();
  public int Id { get; private set; }
  public string Nome { get; private set; }
  public string Email { get; private set; }
  public string Senha { get; private set; }
  public DateOnly DataCriacao { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
  public string Role { get; private set; } = Roles.DESBRAVADOR.ToString();

  public ICollection<UsuarioSessao> Sessoes { get; private set; } = new List<UsuarioSessao>();

  public Usuario(string nome, string email, string senha, string role)
  {
    Nome = nome;
    Email = email;
    Senha = senha;
    this.Role = role;
  }
  public Usuario(Guid id, string nome, string email, string senha)
  {
    Uuid = id;
    Nome = nome;
    Email = email;
    Senha = senha;
  }
  public Usuario(Guid id, string nome, string email, string senha, string role)
  {
    Uuid = id;
    Nome = nome;
    Email = email;
    Senha = senha;
    Role = role;
  }

}