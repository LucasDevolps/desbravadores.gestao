using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.UnitTests.TestDoubles;

namespace Desbravadores.Gestao.UnitTests.Application;

public sealed class UsuariosHandlersTests
{
  [Fact]
  public async Task CriarUsuario_creates_user_with_hash_and_role()
  {
    var repository = new FakeUsuarioRepository();
    var passwordHasher = new FakePasswordHasher();
    var handler = new CriarUsuarioCommandHandler(repository, passwordHasher);

    var id = await handler.Handle(
      new CriarUsuarioCommand("Lucas", "lucas@email.com", "123456", "DIRETORIA"),
      CancellationToken.None);

    var usuario = Assert.Single(repository.UsuariosAdicionados);
    Assert.Equal(usuario.Uuid, id);
    Assert.Equal("Lucas", usuario.Nome);
    Assert.Equal("lucas@email.com", usuario.Email);
    Assert.Equal("hashed:123456", usuario.Senha);
    Assert.Equal(Roles.DIRETORIA, usuario.Role);
    Assert.Equal("123456", Assert.Single(passwordHasher.SenhasHasheadas));
  }

  [Fact]
  public async Task CriarUsuario_uses_default_role_when_parse_fails()
  {
    var repository = new FakeUsuarioRepository();
    var handler = new CriarUsuarioCommandHandler(repository, new FakePasswordHasher());

    await handler.Handle(
      new CriarUsuarioCommand("Lucas", "lucas@email.com", "123456", "invalid"),
      CancellationToken.None);

    Assert.Equal(Roles.DESBRAVADOR, Assert.Single(repository.UsuariosAdicionados).Role);
  }

  [Fact]
  public async Task CriarUsuario_throws_when_email_already_exists()
  {
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(email: "lucas@email.com"));
    var passwordHasher = new FakePasswordHasher();
    var handler = new CriarUsuarioCommandHandler(repository, passwordHasher);

    await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
      new CriarUsuarioCommand("Lucas", "lucas@email.com", "123456", "DIRETORIA"),
      CancellationToken.None));

    Assert.Empty(repository.UsuariosAdicionados);
    Assert.Empty(passwordHasher.SenhasHasheadas);
  }

  [Fact]
  public async Task AtualizarUsuario_updates_all_informed_fields()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(
      uuid: uuid,
      nome: "Lucas",
      email: "lucas@email.com",
      senha: "old-hash",
      role: Roles.DESBRAVADOR));
    var passwordHasher = new FakePasswordHasher();
    var handler = new AtualizarUsuarioCommandHandler(repository, passwordHasher);
    var before = DateTime.UtcNow;

    var dto = await handler.Handle(
      new AtualizarUsuarioCommand(uuid, "  Maria  ", "  MARIA@EMAIL.COM  ", "nova-senha", "SECRETARIA", "127.0.0.1", uuid),
      CancellationToken.None);
    var after = DateTime.UtcNow;

    var usuario = Assert.Single(repository.Usuarios);
    Assert.Equal("Maria", usuario.Nome);
    Assert.Equal("maria@email.com", usuario.Email);
    Assert.Equal("hashed:nova-senha", usuario.Senha);
    Assert.Equal(Roles.SECRETARIA, usuario.Role);
    Assert.NotNull(usuario.DataAtualizacao);
    Assert.InRange(usuario.DataAtualizacao.Value, before, after);
    Assert.Equal(usuario.Id, usuario.UsuarioLogadoId);
    Assert.Same(usuario, usuario.UsuarioLogado);
    Assert.Equal("127.0.0.1", usuario.IpUsuarioLogado);
    Assert.Equal(usuario.Uuid, dto.Id);
    Assert.Equal(usuario.Role, dto.Roles);
    Assert.Equal(usuario.DataAtualizacao, dto.DataAtualizacao);
    Assert.Equal(usuario.Uuid, dto.UsuarioLogado);
    Assert.Equal(usuario.IpUsuarioLogado, dto.IpUsuarioLogado);
    Assert.Equal(1, repository.SaveChangesChamadas);
  }

  [Fact]
  public async Task AtualizarUsuario_ignores_blank_optional_fields_but_saves()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(
      uuid: uuid,
      nome: "Lucas",
      email: "lucas@email.com",
      senha: "old-hash",
      role: Roles.DESBRAVADOR));
    var passwordHasher = new FakePasswordHasher();
    var handler = new AtualizarUsuarioCommandHandler(repository, passwordHasher);

    var dto = await handler.Handle(
      new AtualizarUsuarioCommand(uuid, " ", null, "", null, "10.0.0.1", uuid),
      CancellationToken.None);

    var usuario = Assert.Single(repository.Usuarios);
    Assert.Equal("Lucas", usuario.Nome);
    Assert.Equal("lucas@email.com", usuario.Email);
    Assert.Equal("old-hash", usuario.Senha);
    Assert.Equal(Roles.DESBRAVADOR, usuario.Role);
    Assert.NotNull(usuario.DataAtualizacao);
    Assert.Equal(usuario.Id, usuario.UsuarioLogadoId);
    Assert.Equal("10.0.0.1", usuario.IpUsuarioLogado);
    Assert.Equal(usuario.Nome, dto.Nome);
    Assert.Empty(passwordHasher.SenhasHasheadas);
    Assert.Equal(1, repository.SaveChangesChamadas);
  }

  [Fact]
  public async Task AtualizarUsuario_throws_when_user_does_not_exist()
  {
    var usuarioLogadoUuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: usuarioLogadoUuid));
    var handler = new AtualizarUsuarioCommandHandler(
      repository,
      new FakePasswordHasher());

    await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(
      new AtualizarUsuarioCommand(Guid.NewGuid(), "Lucas", null, null, null, "127.0.0.1", usuarioLogadoUuid),
      CancellationToken.None));
  }

  [Fact]
  public async Task AtualizarUsuario_throws_when_email_belongs_to_another_user()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: uuid, id: 1, email: "lucas@email.com"));
    repository.Usuarios.Add(UsuarioTestFactory.Create(id: 2, email: "maria@email.com"));
    var handler = new AtualizarUsuarioCommandHandler(repository, new FakePasswordHasher());

    await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
      new AtualizarUsuarioCommand(uuid, null, "maria@email.com", null, null, "127.0.0.1", uuid),
      CancellationToken.None));

    Assert.Equal(0, repository.SaveChangesChamadas);
  }

  [Fact]
  public async Task AtualizarUsuario_accepts_current_users_same_email()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: uuid, email: "lucas@email.com"));
    var handler = new AtualizarUsuarioCommandHandler(repository, new FakePasswordHasher());

    var dto = await handler.Handle(
      new AtualizarUsuarioCommand(uuid, null, "  LUCAS@EMAIL.COM  ", null, null, "127.0.0.1", uuid),
      CancellationToken.None);

    Assert.Equal("lucas@email.com", dto.Email);
    Assert.Equal(1, repository.SaveChangesChamadas);
  }

  [Fact]
  public async Task AtualizarUsuario_throws_when_role_is_invalid()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: uuid));
    var handler = new AtualizarUsuarioCommandHandler(repository, new FakePasswordHasher());

    await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
      new AtualizarUsuarioCommand(uuid, null, null, null, "invalid", "127.0.0.1", uuid),
      CancellationToken.None));
  }

  [Fact]
  public async Task AtualizarUsuario_throws_when_logged_user_does_not_exist()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: uuid));
    var handler = new AtualizarUsuarioCommandHandler(repository, new FakePasswordHasher());

    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
      new AtualizarUsuarioCommand(uuid, null, null, null, null, "127.0.0.1", Guid.NewGuid()),
      CancellationToken.None));

    Assert.Equal(0, repository.SaveChangesChamadas);
  }

  [Fact]
  public async Task BuscaPorId_returns_user_dto()
  {
    var uuid = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(uuid: uuid, nome: "Lucas"));
    var handler = new BuscaPorIdQueryHandler(repository);

    var dto = await handler.Handle(new BuscaUsuarioPorIdQuery(uuid), CancellationToken.None);

    Assert.Equal(uuid, dto.Id);
    Assert.Equal("Lucas", dto.Nome);
  }

  [Fact]
  public async Task BuscaPorId_throws_when_user_does_not_exist()
  {
    var handler = new BuscaPorIdQueryHandler(new FakeUsuarioRepository());

    await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      handler.Handle(new BuscaUsuarioPorIdQuery(Guid.NewGuid()), CancellationToken.None));
  }

  [Fact]
  public async Task BuscaUsuarioPorIdQuery_recuperar_id_replaces_id()
  {
    var query = new BuscaUsuarioPorIdQuery();
    var id = Guid.NewGuid();

    query.RecuperarId(id);

    Assert.Equal(id, query.Id);
  }

  [Fact]
  public async Task GetAll_returns_repository_users()
  {
    var repository = new FakeUsuarioRepository();
    repository.Usuarios.Add(UsuarioTestFactory.Create(nome: "Lucas"));
    repository.Usuarios.Add(UsuarioTestFactory.Create(nome: "Maria"));
    var handler = new GetAllUsuariosQueryHandler(repository);

    var result = (await handler.Handle(new GetAllUsuariosQuery(), CancellationToken.None)).ToList();

    Assert.Equal(2, result.Count);
    Assert.Contains(result, x => x.Nome == "Lucas");
    Assert.Contains(result, x => x.Nome == "Maria");
  }

  [Fact]
  public async Task DeletarUsuario_delegates_to_repository()
  {
    var id = Guid.NewGuid();
    var repository = new FakeUsuarioRepository();
    var handler = new DeletarUsuarioCommandHandler(repository);

    await handler.Handle(new DeletarUsuarioCommand(id), CancellationToken.None);

    Assert.Equal(id, Assert.Single(repository.UsuariosDeletados));
  }
}
