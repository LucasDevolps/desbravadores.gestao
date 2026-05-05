using Desbravadores.Gestao.Application.Interfaces;

namespace Desbravadores.Gestao.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
  private const int WorkFactor = 12;
  private const string PepperEnvironmentKey = "PASSWORD_PEPPER";
  private const string EnhancedPrefix = "desb:bcrypt-sha384:v2:";
  private const string PepperedPrefix = "desb:bcrypt-sha384-pepper:v2:";

  private readonly string? _pepper;

  public PasswordHasher()
    : this(Environment.GetEnvironmentVariable(PepperEnvironmentKey))
  {
  }

  public PasswordHasher(string? pepper)
  {
    _pepper = string.IsNullOrWhiteSpace(pepper) ? null : pepper;
  }

  public Task<string> HashAsync(string password, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var usePepper = _pepper is not null;
    var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(
      BuildPasswordInput(password, usePepper),
      WorkFactor,
      BCrypt.Net.HashType.SHA384);

    return Task.FromResult($"{(usePepper ? PepperedPrefix : EnhancedPrefix)}{hash}");
  }

  public Task<bool> VerifyAsync(string password, string passwordHash, CancellationToken cancellationToken = default)
  {
    cancellationToken.ThrowIfCancellationRequested();

    return Task.FromResult(VerifyPassword(password, passwordHash));
  }

  public bool NeedsRehash(string passwordHash)
  {
    if (string.IsNullOrWhiteSpace(passwordHash))
      return true;

    if (passwordHash.StartsWith(PepperedPrefix, StringComparison.Ordinal))
      return !HasCurrentWorkFactor(passwordHash[PepperedPrefix.Length..]);

    if (passwordHash.StartsWith(EnhancedPrefix, StringComparison.Ordinal))
      return _pepper is not null || !HasCurrentWorkFactor(passwordHash[EnhancedPrefix.Length..]);

    return true;
  }

  private bool VerifyPassword(string password, string passwordHash)
  {
    if (string.IsNullOrWhiteSpace(passwordHash))
      return false;

    try
    {
      if (passwordHash.StartsWith(PepperedPrefix, StringComparison.Ordinal))
      {
        if (_pepper is null)
          return false;

        return BCrypt.Net.BCrypt.EnhancedVerify(
          BuildPasswordInput(password, usePepper: true),
          passwordHash[PepperedPrefix.Length..],
          BCrypt.Net.HashType.SHA384);
      }

      if (passwordHash.StartsWith(EnhancedPrefix, StringComparison.Ordinal))
      {
        return BCrypt.Net.BCrypt.EnhancedVerify(
          password,
          passwordHash[EnhancedPrefix.Length..],
          BCrypt.Net.HashType.SHA384);
      }

      return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
    catch (ArgumentException)
    {
      return false;
    }
    catch (BCrypt.Net.SaltParseException)
    {
      return false;
    }
  }

  private string BuildPasswordInput(string password, bool usePepper)
  {
    return usePepper && _pepper is not null
      ? $"{_pepper}:{password}"
      : password;
  }

  private static bool HasCurrentWorkFactor(string bcryptHash)
  {
    var hashParts = bcryptHash.Split('$', StringSplitOptions.RemoveEmptyEntries);

    return hashParts.Length >= 2
      && int.TryParse(hashParts[1], out var workFactor)
      && workFactor >= WorkFactor;
  }
}
