using Desbravadores.Gestao.Application.Interfaces;

namespace Desbravadores.Gestao.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
  public async Task<string> HashAsync(string password, CancellationToken cancellationToken = default)
  {
    return BCrypt.Net.BCrypt.HashPassword(password);
  }

  public async Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken = default)
  {
    return BCrypt.Net.BCrypt.Verify(password, passwordHash);
  }
}
