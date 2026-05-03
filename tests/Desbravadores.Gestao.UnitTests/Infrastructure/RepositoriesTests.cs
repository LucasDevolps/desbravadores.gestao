using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;
using Desbravadores.Gestao.Infrastructure.Data;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.UnitTests.Infrastructure;

public sealed class RepositoriesTests
{
  [Fact]
  public async Task UsuarioRepository_adds_and_reads_users()
  {
    await using var context = await CreateContextAsync();
    var repository = new UsuarioRepository(context);
    var usuario = new Usuario("Lucas", "lucas@email.com", "hash", Roles.SECRETARIA);

    await repository.AdicionarUsuarioAsync(usuario, CancellationToken.None);

    var byEmail = await repository.GetByEmailAsync("  LUCAS@EMAIL.COM  ", CancellationToken.None);
    var byUuid = await repository.GetByUuidAsync(usuario.Uuid, CancellationToken.None);
    var byId = await repository.GetByIdAsync(usuario.Uuid, CancellationToken.None);
    var all = (await repository.GetAllAsync(CancellationToken.None)).ToList();

    Assert.NotEqual(0, usuario.Id);
    Assert.Equal(usuario.Uuid, byEmail!.Uuid);
    Assert.Equal(usuario.Uuid, byUuid!.Uuid);
    Assert.Equal(usuario.Uuid, byId!.Id);
    Assert.Equal("Lucas", byId.Nome);
    Assert.Single(all);
    Assert.Equal(usuario.Uuid, all[0].Id);
  }

  [Fact]
  public async Task UsuarioRepository_delete_removes_existing_user_and_ignores_missing_user()
  {
    await using var context = await CreateContextAsync();
    var repository = new UsuarioRepository(context);
    var usuario = new Usuario("Lucas", "lucas@email.com", "hash");
    await repository.AdicionarUsuarioAsync(usuario, CancellationToken.None);

    await repository.DeletarUsuarioAsync(usuario.Uuid, CancellationToken.None);
    await repository.DeletarUsuarioAsync(Guid.NewGuid(), CancellationToken.None);

    Assert.Null(await repository.GetByUuidAsync(usuario.Uuid, CancellationToken.None));
  }

  [Fact]
  public async Task UsuarioRepository_save_changes_persists_tracked_updates()
  {
    await using var context = await CreateContextAsync();
    var repository = new UsuarioRepository(context);
    var usuario = new Usuario("Lucas", "lucas@email.com", "hash");
    await repository.AdicionarUsuarioAsync(usuario, CancellationToken.None);

    usuario.AtualizarNome("Maria");
    await repository.SaveChangesAsync(CancellationToken.None);
    context.ChangeTracker.Clear();

    var reloaded = await repository.GetByUuidAsync(usuario.Uuid, CancellationToken.None);
    Assert.Equal("Maria", reloaded!.Nome);
  }

  [Fact]
  public async Task UsuarioSessaoRepository_adds_and_finds_sessions()
  {
    await using var context = await CreateContextAsync();
    var usuario = await AddUserAsync(context);
    var repository = new UsuarioSessaoRepository(context);
    var sessao = new UsuarioSessao(
      usuario.Id,
      "jti-active",
      "refresh-active",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));

    await repository.AddAsync(sessao, CancellationToken.None);
    await repository.SaveChangesAsync(CancellationToken.None);
    context.ChangeTracker.Clear();

    var byJti = await repository.GetByJtiAsync("jti-active", CancellationToken.None);
    var byRefresh = await repository.GetByRefreshTokenAsync("refresh-active", CancellationToken.None);

    Assert.NotNull(byJti);
    Assert.Equal("jti-active", byJti!.Jti);
    Assert.NotNull(byRefresh);
    Assert.Equal(usuario.Uuid, byRefresh!.Usuario.Uuid);
  }

  [Fact]
  public async Task UsuarioSessaoRepository_exists_active_session_checks_jti_revocation_and_expiration()
  {
    await using var context = await CreateContextAsync();
    var usuario = await AddUserAsync(context);
    var repository = new UsuarioSessaoRepository(context);
    var active = new UsuarioSessao(
      usuario.Id,
      "jti-active",
      "refresh-active",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    var expired = new UsuarioSessao(
      usuario.Id,
      "jti-expired",
      "refresh-expired",
      DateTime.UtcNow.AddMinutes(-1),
      DateTime.UtcNow.AddDays(7));
    var revoked = new UsuarioSessao(
      usuario.Id,
      "jti-revoked",
      "refresh-revoked",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    revoked.Revogar();

    context.UsuarioSessoes.AddRange(active, expired, revoked);
    await context.SaveChangesAsync();

    Assert.True(await repository.ExistsActiveSessionAsync(usuario.Id, "jti-active", CancellationToken.None));
    Assert.False(await repository.ExistsActiveSessionAsync(usuario.Id, "missing", CancellationToken.None));
    Assert.False(await repository.ExistsActiveSessionAsync(usuario.Id, "jti-expired", CancellationToken.None));
    Assert.False(await repository.ExistsActiveSessionAsync(usuario.Id, "jti-revoked", CancellationToken.None));
  }

  [Fact]
  public async Task UsuarioSessaoRepository_revoke_all_active_revokes_only_current_users_unexpired_refresh_sessions()
  {
    await using var context = await CreateContextAsync();
    var usuario = await AddUserAsync(context, "lucas@email.com");
    var outroUsuario = await AddUserAsync(context, "maria@email.com");
    var active = new UsuarioSessao(
      usuario.Id,
      "jti-active",
      "refresh-active",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    var expiredRefresh = new UsuarioSessao(
      usuario.Id,
      "jti-expired-refresh",
      "refresh-expired-refresh",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(-1));
    var otherUser = new UsuarioSessao(
      outroUsuario.Id,
      "jti-other",
      "refresh-other",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    context.UsuarioSessoes.AddRange(active, expiredRefresh, otherUser);
    await context.SaveChangesAsync();
    var repository = new UsuarioSessaoRepository(context);

    await repository.RevokeAllActiveByUsuarioIdAsync(usuario.Id, CancellationToken.None);
    context.ChangeTracker.Clear();

    var sessions = await context.UsuarioSessoes.ToDictionaryAsync(x => x.Jti);
    Assert.True(sessions["jti-active"].Revogado);
    Assert.NotNull(sessions["jti-active"].DataRevogacao);
    Assert.False(sessions["jti-expired-refresh"].Revogado);
    Assert.False(sessions["jti-other"].Revogado);
  }

  private static async Task<AppDbContext> CreateContextAsync()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    var context = new AppDbContext(options);
    await context.Database.OpenConnectionAsync();
    await context.Database.EnsureCreatedAsync();
    return context;
  }

  private static async Task<Usuario> AddUserAsync(AppDbContext context, string email = "lucas@email.com")
  {
    var usuario = new Usuario("Lucas", email, "hash", Roles.DESBRAVADOR);
    context.Usuarios.Add(usuario);
    await context.SaveChangesAsync();
    return usuario;
  }
}
