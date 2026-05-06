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
  public DateTime? DataAtualizacao { get; private set; }
  public int? UsuarioLogadoId { get; private set; }
  public string? IpUsuarioLogado { get; private set; }
  public Roles Role { get; private set; } = Roles.DESBRAVADOR;

  public ICollection<UsuarioSessao> Sessoes { get; private set; } = new List<UsuarioSessao>();
  public Usuario? UsuarioLogado { get; private set; }

  public Usuario(string nome, string email, string senha, Roles role = Roles.DESBRAVADOR)
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
  public Usuario(Guid id, string nome, string email, string senha, Roles role = Roles.DESBRAVADOR)
  {
    Uuid = id;
    Nome = nome;
    Email = email;
    Senha = senha;
    Role = role;
  }
  public void AtualizarNome(string nome)
  {
    Nome = nome.Trim();
  }

  public void AtualizarEmail(string email)
  {
    Email = email.Trim().ToLowerInvariant();
  }

  public void AtualizarSenha(string senhaHash)
  {
    Senha = senhaHash;
  }

  public void AtualizarRole(Roles role)
  {
    Role = role;
  }

  public void RegistrarAtualizacao(Usuario usuarioLogado, string ipUsuarioLogado)
  {
    if (usuarioLogado.Id <= 0)
      throw new InvalidOperationException("Usuário logado inválido.");

    if (string.IsNullOrWhiteSpace(ipUsuarioLogado))
      throw new InvalidOperationException("IP do usuário logado é obrigatório.");

    DataAtualizacao = DateTime.UtcNow;
    UsuarioLogadoId = usuarioLogado.Id;
    UsuarioLogado = usuarioLogado;
    IpUsuarioLogado = ipUsuarioLogado.Trim();
  }

}
