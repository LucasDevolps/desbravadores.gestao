using System.IdentityModel.Tokens.Jwt;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Infrastructure.Security;
using Desbravadores.Gestao.UnitTests.TestDoubles;

namespace Desbravadores.Gestao.UnitTests.Infrastructure;

public sealed class SecurityTests
{
  private static readonly string[] JwtEnvironmentKeys =
  [
    "JWT_KEY",
    "JWT_SECRET",
    "JWT_AUDIENCE",
    "JWT_ISSUER",
    "Jwt_ExpiresInMinutes",
    "Jwt_RefreshTokenExpiresInDays"
  ];

  [Fact]
  public async Task PasswordHasher_hashes_and_verifies_passwords()
  {
    var hasher = new PasswordHasher(pepper: null);

    var hash = await hasher.HashAsync("123456");

    Assert.StartsWith("desb:bcrypt-sha384:v2:", hash);
    Assert.Contains("$12$", hash);
    Assert.NotEqual("123456", hash);
    Assert.True(await hasher.VerifyAsync("123456", hash));
    Assert.False(await hasher.VerifyAsync("wrong", hash));
    Assert.False(hasher.NeedsRehash(hash));
  }

  [Fact]
  public async Task PasswordHasher_verifies_legacy_bcrypt_hashes()
  {
    var hasher = new PasswordHasher(pepper: null);
    var legacyHash = BCrypt.Net.BCrypt.HashPassword("123456", 4);

    Assert.True(await hasher.VerifyAsync("123456", legacyHash));
    Assert.False(await hasher.VerifyAsync("wrong", legacyHash));
    Assert.True(hasher.NeedsRehash(legacyHash));
  }

  [Fact]
  public async Task PasswordHasher_uses_pepper_when_configured()
  {
    var hasher = new PasswordHasher("pepper-secreto");

    var hash = await hasher.HashAsync("123456");

    Assert.StartsWith("desb:bcrypt-sha384-pepper:v2:", hash);
    Assert.True(await hasher.VerifyAsync("123456", hash));
    Assert.False(await new PasswordHasher("outro-pepper").VerifyAsync("123456", hash));
    Assert.False(await new PasswordHasher(pepper: null).VerifyAsync("123456", hash));
    Assert.False(hasher.NeedsRehash(hash));
  }

  [Fact]
  public async Task PasswordHasher_handles_invalid_hash_as_failed_verification()
  {
    var hasher = new PasswordHasher(pepper: null);

    Assert.False(await hasher.VerifyAsync("123456", "hash-invalido"));
    Assert.True(hasher.NeedsRehash("hash-invalido"));
  }

  [Fact]
  public void TokenService_generate_refresh_token_returns_random_base64_value()
  {
    var service = new TokenService();

    var first = service.GenerateRefreshToken();
    var second = service.GenerateRefreshToken();

    Assert.NotEqual(first, second);
    Assert.Equal(64, Convert.FromBase64String(first).Length);
    Assert.Equal(64, Convert.FromBase64String(second).Length);
  }

  [Fact]
  public void TokenService_required_environment_values_throw_when_missing()
  {
    WithEnvironment(
      values: [],
      action: () =>
      {
        var service = new TokenService();

        Assert.Throws<InvalidOperationException>(() => service.GetJWT_KEY());
        Assert.Throws<InvalidOperationException>(() => service.GetJWT_SECRET());
        Assert.Throws<InvalidOperationException>(() => service.GetJWT_AUDIENCE());
        Assert.Throws<InvalidOperationException>(() => service.GetJWT_ISSUER());
      });
  }

  [Fact]
  public void TokenService_uses_default_expiration_values_when_optional_env_is_invalid()
  {
    WithEnvironment(
      new Dictionary<string, string?>
      {
        ["Jwt_ExpiresInMinutes"] = "invalid",
        ["Jwt_RefreshTokenExpiresInDays"] = "invalid"
      },
      () =>
      {
        var service = new TokenService();

        Assert.Equal(15, service.GetJWT_ACCESS_TOKEN_MINUTES());
        Assert.Equal(7, service.GetJWT_REFRESH_TOKEN_DAYS());
      });
  }

  [Fact]
  public async Task TokenService_generate_token_contains_expected_claims_and_expiration()
  {
    var key = new string('a', 64);
    var usuario = UsuarioTestFactory.Create(
      uuid: Guid.NewGuid(),
      id: 123,
      nome: "Lucas",
      email: "lucas@email.com",
      role: Roles.DIRETORIA);

    await WithEnvironmentAsync(
      new Dictionary<string, string?>
      {
        ["JWT_KEY"] = key,
        ["JWT_SECRET"] = "secret",
        ["JWT_AUDIENCE"] = "desbravadores-audience",
        ["JWT_ISSUER"] = "desbravadores-issuer",
        ["Jwt_ExpiresInMinutes"] = "30",
        ["Jwt_RefreshTokenExpiresInDays"] = "10"
      },
      async () =>
      {
        var before = DateTime.UtcNow;
        var service = new TokenService();

        var result = await service.GenerateToken(usuario);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        Assert.Equal("desbravadores-issuer", jwt.Issuer);
        Assert.Contains("desbravadores-audience", jwt.Audiences);
        Assert.Contains(jwt.Claims, x => x.Type == JwtRegisteredClaimNames.Sub && x.Value == usuario.Uuid.ToString());
        Assert.Contains(jwt.Claims, x => x.Type == JwtRegisteredClaimNames.Email && x.Value == usuario.Email);
        Assert.Contains(jwt.Claims, x => x.Type == JwtRegisteredClaimNames.Jti && x.Value == result.Jti);
        Assert.Contains(jwt.Claims, x => x.Type == "user_id" && x.Value == usuario.Id.ToString());
        Assert.NotEqual(string.Empty, result.RefreshToken);
        Assert.Equal(64, Convert.FromBase64String(result.RefreshToken).Length);
        Assert.InRange(result.AccessTokenExpiraEm, before.AddMinutes(30), DateTime.UtcNow.AddMinutes(30).AddSeconds(2));
        Assert.InRange(result.RefreshTokenExpiraEm, before.AddDays(10), DateTime.UtcNow.AddDays(10).AddSeconds(2));
      });
  }

  private static void WithEnvironment(Dictionary<string, string?> values, Action action)
  {
    var previousValues = CaptureEnvironment();

    try
    {
      ClearEnvironment();
      foreach (var pair in values)
        Environment.SetEnvironmentVariable(pair.Key, pair.Value);

      action();
    }
    finally
    {
      RestoreEnvironment(previousValues);
    }
  }

  private static async Task WithEnvironmentAsync(Dictionary<string, string?> values, Func<Task> action)
  {
    var previousValues = CaptureEnvironment();

    try
    {
      ClearEnvironment();
      foreach (var pair in values)
        Environment.SetEnvironmentVariable(pair.Key, pair.Value);

      await action();
    }
    finally
    {
      RestoreEnvironment(previousValues);
    }
  }

  private static Dictionary<string, string?> CaptureEnvironment()
  {
    return JwtEnvironmentKeys.ToDictionary(
      key => key,
      Environment.GetEnvironmentVariable);
  }

  private static void ClearEnvironment()
  {
    foreach (var key in JwtEnvironmentKeys)
      Environment.SetEnvironmentVariable(key, null);
  }

  private static void RestoreEnvironment(Dictionary<string, string?> values)
  {
    foreach (var pair in values)
      Environment.SetEnvironmentVariable(pair.Key, pair.Value);
  }
}
