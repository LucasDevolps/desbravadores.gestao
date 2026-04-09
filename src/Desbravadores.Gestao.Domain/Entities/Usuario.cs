namespace Desbravadores.Gestao.Domain.Entities;

public sealed class Usuario(string nome, string email, string senha)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = nome;
    public string Email { get; private set; } = email;
    public string Senha { get; private set; } = senha;
    public DateOnly DataCriacao { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);

}
