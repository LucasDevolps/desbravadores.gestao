using Desbravadores.Gestao.Application.UseCases.Auth.Login;
using Desbravadores.Gestao.Application.UseCases.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Application.UseCases.Auth.Refresh;
using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using FluentValidation;

namespace Desbravadores.Gestao.UnitTests.Application;

public sealed class ValidatorsTests
{
  [Fact]
  public void CriarUsuarioValidator_accepts_valid_command_and_case_insensitive_role()
  {
    var validator = new CriarUsuarioCommandValidator();

    var result = validator.Validate(new CriarUsuarioCommand(
      "Lucas",
      "lucas@email.com",
      "123456",
      "diretoria"));

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData("", "lucas@email.com", "123456", "DIRETORIA")]
  [InlineData("Lucas", "email-invalido", "123456", "DIRETORIA")]
  [InlineData("Lucas", "lucas@email.com", "", "DIRETORIA")]
  [InlineData("Lucas", "lucas@email.com", "123456", "INVALID")]
  public void CriarUsuarioValidator_rejects_invalid_command(
    string nome,
    string email,
    string senha,
    string role)
  {
    var validator = new CriarUsuarioCommandValidator();

    var result = validator.Validate(new CriarUsuarioCommand(nome, email, senha, role));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void LoginValidator_accepts_valid_command()
  {
    var validator = CreateValidator<LoginCommand>("LoginCommandValidator");

    var result = validator.Validate(new LoginCommand("lucas@email.com", "123456"));

    Assert.True(result.IsValid);
  }

  [Theory]
  [InlineData("email-invalido", "123456")]
  [InlineData("lucas@email.com", "")]
  public void LoginValidator_rejects_invalid_command(string email, string senha)
  {
    var validator = CreateValidator<LoginCommand>("LoginCommandValidator");

    var result = validator.Validate(new LoginCommand(email, senha));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void LogoutValidator_requires_jti()
  {
    var validator = CreateValidator<LogoutCommand>("LogoutCommandValidator");

    var invalid = validator.Validate(new LogoutCommand(""));
    var valid = validator.Validate(new LogoutCommand("jti"));

    Assert.False(invalid.IsValid);
    Assert.True(valid.IsValid);
  }

  [Fact]
  public void MeValidator_requires_sub_and_jti()
  {
    var validator = CreateValidator<MeQuery>("MeQueryValidator");

    var invalid = validator.Validate(new MeQuery("", ""));
    var valid = validator.Validate(new MeQuery(Guid.NewGuid().ToString(), "jti"));

    Assert.False(invalid.IsValid);
    Assert.True(valid.IsValid);
  }

  [Fact]
  public void RefreshValidator_requires_sub_and_jti()
  {
    var validator = CreateValidator<RefreshQuery>("RefreshQueryValidator");

    var invalid = validator.Validate(new RefreshQuery("", ""));
    var valid = validator.Validate(new RefreshQuery(Guid.NewGuid().ToString(), "jti"));

    Assert.False(invalid.IsValid);
    Assert.True(valid.IsValid);
  }

  [Fact]
  public void BuscaPorIdValidator_requires_non_empty_guid()
  {
    var validator = CreateValidator<BuscaUsuarioPorIdQuery>("BuscaPorIdQueryValidator");

    var invalid = validator.Validate(new BuscaUsuarioPorIdQuery(Guid.Empty));
    var valid = validator.Validate(new BuscaUsuarioPorIdQuery(Guid.NewGuid()));

    Assert.False(invalid.IsValid);
    Assert.True(valid.IsValid);
  }

  [Fact]
  public void DeletarUsuarioValidator_requires_non_empty_guid()
  {
    var validator = CreateValidator<DeletarUsuarioCommand>("DeletarusuarioCommandValidator");

    var invalid = validator.Validate(new DeletarUsuarioCommand(Guid.Empty));
    var valid = validator.Validate(new DeletarUsuarioCommand(Guid.NewGuid()));

    Assert.False(invalid.IsValid);
    Assert.True(valid.IsValid);
  }

  [Fact]
  public void AtualizarUsuarioValidator_accepts_complete_valid_command()
  {
    var validator = CreateValidator<AtualizarUsuarioCommand>("AtualizarUsuarioCommandValidator");

    var result = validator.Validate(new AtualizarUsuarioCommand(
      Guid.NewGuid(),
      "Lucas",
      "lucas@email.com",
      "123456",
      "SECRETARIA"));

    Assert.True(result.IsValid);
  }

  [Fact]
  public void AtualizarUsuarioValidator_allows_null_password_and_blank_role()
  {
    var validator = CreateValidator<AtualizarUsuarioCommand>("AtualizarUsuarioCommandValidator");

    var result = validator.Validate(new AtualizarUsuarioCommand(
      Guid.NewGuid(),
      "Lucas",
      "lucas@email.com",
      null,
      " "));

    Assert.True(result.IsValid);
  }

  [Fact]
  public void AtualizarUsuarioValidator_rejects_partial_command_without_name_or_email()
  {
    var validator = CreateValidator<AtualizarUsuarioCommand>("AtualizarUsuarioCommandValidator");

    var result = validator.Validate(new AtualizarUsuarioCommand(
      Guid.NewGuid(),
      null,
      null,
      null,
      null));

    Assert.False(result.IsValid);
  }

  [Theory]
  [InlineData("", "lucas@email.com", "123456", "SECRETARIA")]
  [InlineData("Lucas", "email-invalido", "123456", "SECRETARIA")]
  [InlineData("Lucas", "lucas@email.com", "123", "SECRETARIA")]
  [InlineData("Lucas", "lucas@email.com", "123456", "INVALID")]
  public void AtualizarUsuarioValidator_rejects_invalid_fields(
    string? nome,
    string? email,
    string? senha,
    string? role)
  {
    var validator = CreateValidator<AtualizarUsuarioCommand>("AtualizarUsuarioCommandValidator");

    var result = validator.Validate(new AtualizarUsuarioCommand(
      Guid.NewGuid(),
      nome,
      email,
      senha,
      role));

    Assert.False(result.IsValid);
  }

  [Fact]
  public void AtualizarUsuarioValidator_rejects_empty_uuid()
  {
    var validator = CreateValidator<AtualizarUsuarioCommand>("AtualizarUsuarioCommandValidator");

    var result = validator.Validate(new AtualizarUsuarioCommand(
      Guid.Empty,
      "Lucas",
      "lucas@email.com",
      "123456",
      "SECRETARIA"));

    Assert.False(result.IsValid);
  }

  private static IValidator<TRequest> CreateValidator<TRequest>(string typeName)
  {
    var validatorType = typeof(TRequest).Assembly
      .GetTypes()
      .Single(x => x.Name == typeName);

    return (IValidator<TRequest>)Activator.CreateInstance(validatorType, nonPublic: true)!;
  }
}
