namespace Desbravadores.Gestao.Domain.Entities;

public sealed class UsuarioSessao
{
  public int Id { get; private set; }
  public Guid Uuid { get; private set; } = Guid.NewGuid();

  public int UsuarioId { get; private set; }
  public Usuario Usuario { get; private set; } = null!;

  public string Jti { get; private set; } = string.Empty;
  public string RefreshToken { get; private set; } = string.Empty;

  public DateTime AccessTokenExpiraEm { get; private set; }
  public DateTime RefreshTokenExpiraEm { get; private set; }

  public bool Revogado { get; private set; }
  public DateTime? DataRevogacao { get; private set; }

  public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;

  private UsuarioSessao() { }

  public UsuarioSessao(
      int usuarioId,
      string jti,
      string refreshToken,
      DateTime accessTokenExpiraEm,
      DateTime refreshTokenExpiraEm)
  {
    UsuarioId = usuarioId;
    Jti = jti;
    RefreshToken = refreshToken;
    AccessTokenExpiraEm = accessTokenExpiraEm;
    RefreshTokenExpiraEm = refreshTokenExpiraEm;
    Revogado = false;
    DataCriacao = DateTime.UtcNow;
  }

  public void Revogar()
  {
    Revogado = true;
    DataRevogacao = DateTime.UtcNow;
  }

  public void AtualizarRefreshToken(string novoRefreshToken, DateTime novaExpiracao)
  {
    RefreshToken = novoRefreshToken;
    RefreshTokenExpiraEm = novaExpiracao;
  }
}
