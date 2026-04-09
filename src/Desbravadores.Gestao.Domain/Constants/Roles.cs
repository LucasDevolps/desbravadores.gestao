namespace Desbravadores.Gestao.Domain.Constants;

public sealed class Roles
{
  public const string DIRETORA = "DIRETORA";
  public const string SECRETARIA = "SECRETARIA";
  public const string TESOURARIA = "TESOURARIA";
  public const string DIRETORIA = "DIRETORIA";
  public const string DESBRAVADOR = "DESBRAVADOR";

  public static readonly HashSet<string> Validas =
    [
        DIRETORA,
        SECRETARIA,
        TESOURARIA,
        DIRETORIA,
        DESBRAVADOR 
    ];
}
