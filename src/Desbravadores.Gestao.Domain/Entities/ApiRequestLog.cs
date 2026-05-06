namespace Desbravadores.Gestao.Domain.Entities;

public sealed class ApiRequestLog
{
  public long Id { get; set; }
  public DateTime DataHora { get; set; }
  public string MetodoHttp { get; set; } = string.Empty;
  public string Endpoint { get; set; } = string.Empty;
  public string? QueryString { get; set; }
  public string? RequestHeaders { get; set; }
  public string? RequestBody { get; set; }
  public int StatusCode { get; set; }
  public string? ResponseBody { get; set; }
  public long TempoExecucaoMs { get; set; }
  public string? UsuarioId { get; set; }
  public string? UsuarioEmail { get; set; }
  public string? CorrelationId { get; set; }
  public string? TraceId { get; set; }
  public string? IpOrigem { get; set; }
  public bool Sucesso { get; set; }
  public string? ExceptionType { get; set; }
  public string? ExceptionMessage { get; set; }
  public string? StackTrace { get; set; }
  public string? Ambiente { get; set; }
}
