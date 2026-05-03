using Desbravadores.Gestao.Application.DTOs;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.UnitTests.Domain;

public sealed class DomainModelsTests
{
  [Fact]
  public void Usuario_constructor_sets_defaults()
  {
    var before = DateOnly.FromDateTime(DateTime.UtcNow);

    var usuario = new Usuario("Lucas", "lucas@email.com", "hash");

    var after = DateOnly.FromDateTime(DateTime.UtcNow);
    Assert.NotEqual(Guid.Empty, usuario.Uuid);
    Assert.Equal(0, usuario.Id);
    Assert.Equal("Lucas", usuario.Nome);
    Assert.Equal("lucas@email.com", usuario.Email);
    Assert.Equal("hash", usuario.Senha);
    Assert.Equal(Roles.DESBRAVADOR, usuario.Role);
    Assert.InRange(usuario.DataCriacao, before, after);
    Assert.Empty(usuario.Sessoes);
  }

  [Fact]
  public void Usuario_constructor_accepts_uuid_and_role()
  {
    var uuid = Guid.NewGuid();

    var usuario = new Usuario(uuid, "Maria", "maria@email.com", "hash", Roles.SECRETARIA);

    Assert.Equal(uuid, usuario.Uuid);
    Assert.Equal(Roles.SECRETARIA, usuario.Role);
  }

  [Fact]
  public void Usuario_update_methods_normalize_and_replace_values()
  {
    var usuario = new Usuario("Lucas", "lucas@email.com", "hash");

    usuario.AtualizarNome("  Novo Nome  ");
    usuario.AtualizarEmail("  NOVO@EMAIL.COM  ");
    usuario.AtualizarSenha("new-hash");
    usuario.AtualizarRole(Roles.DIRETORIA);

    Assert.Equal("Novo Nome", usuario.Nome);
    Assert.Equal("novo@email.com", usuario.Email);
    Assert.Equal("new-hash", usuario.Senha);
    Assert.Equal(Roles.DIRETORIA, usuario.Role);
  }

  [Fact]
  public void UsuarioSessao_constructor_sets_active_session()
  {
    var accessExpires = DateTime.UtcNow.AddMinutes(15);
    var refreshExpires = DateTime.UtcNow.AddDays(7);

    var sessao = new UsuarioSessao(10, "jti", "refresh", accessExpires, refreshExpires);

    Assert.Equal(10, sessao.UsuarioId);
    Assert.Equal("jti", sessao.Jti);
    Assert.Equal("refresh", sessao.RefreshToken);
    Assert.Equal(accessExpires, sessao.AccessTokenExpiraEm);
    Assert.Equal(refreshExpires, sessao.RefreshTokenExpiraEm);
    Assert.False(sessao.Revogado);
    Assert.Null(sessao.DataRevogacao);
    Assert.NotEqual(Guid.Empty, sessao.Uuid);
  }

  [Fact]
  public void UsuarioSessao_revogar_marks_session_as_revoked()
  {
    var sessao = new UsuarioSessao(
      10,
      "jti",
      "refresh",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));

    sessao.Revogar();

    Assert.True(sessao.Revogado);
    Assert.NotNull(sessao.DataRevogacao);
  }

  [Fact]
  public void UsuarioSessao_atualizar_refresh_token_replaces_token_and_expiration()
  {
    var sessao = new UsuarioSessao(
      10,
      "jti",
      "refresh",
      DateTime.UtcNow.AddMinutes(15),
      DateTime.UtcNow.AddDays(7));
    var novaExpiracao = DateTime.UtcNow.AddDays(30);

    sessao.AtualizarRefreshToken("novo-refresh", novaExpiracao);

    Assert.Equal("novo-refresh", sessao.RefreshToken);
    Assert.Equal(novaExpiracao, sessao.RefreshTokenExpiraEm);
  }

  [Fact]
  public void UsuarioDTO_from_entity_maps_user_data_and_role()
  {
    var usuario = new Usuario(Guid.NewGuid(), "Lucas", "lucas@email.com", "hash", Roles.TESOURARIA);

    var dto = new UsuarioDTO().FromEntity(usuario);

    Assert.Equal(usuario.Uuid, dto.Id);
    Assert.Equal(usuario.Nome, dto.Nome);
    Assert.Equal(usuario.Email, dto.Email);
    Assert.Equal(usuario.DataCriacao, dto.DataCriacao);
    Assert.Equal(Roles.TESOURARIA, dto.Roles);
  }

  [Fact]
  public void UsuarioDTO_from_entity_without_roles_keeps_default_role_value()
  {
    var usuario = new Usuario(Guid.NewGuid(), "Lucas", "lucas@email.com", "hash", Roles.SECRETARIA);

    var dto = new UsuarioDTO().FromEntityWithOutRoles(usuario);

    Assert.Equal(usuario.Uuid, dto.Id);
    Assert.Equal(usuario.Nome, dto.Nome);
    Assert.Equal(usuario.Email, dto.Email);
    Assert.Equal(usuario.DataCriacao, dto.DataCriacao);
    Assert.Equal(default, dto.Roles);
  }
}
