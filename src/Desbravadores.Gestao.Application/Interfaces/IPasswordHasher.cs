namespace Desbravadores.Gestao.Application.Interfaces;

public interface IPasswordHasher
{
  Task<string> HashAsync(string password, CancellationToken cancellationToken = default);
  Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken = default);
}
