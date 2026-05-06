using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Entities;

namespace Desbravadores.Gestao.Api.Middlewares;

public sealed class RequestResponseLoggingMiddleware(
  RequestDelegate next,
  ILogger<RequestResponseLoggingMiddleware> logger)
{
  private const int MaxBodyLength = 8000;
  private static readonly HashSet<string> SensitiveFields =
  [
    "password",
    "senha",
    "token",
    "accessToken",
    "refreshToken",
    "authorization",
    "cookie",
    "set-cookie"
  ];

  private readonly RequestDelegate _next = next;
  private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;

  public async Task InvokeAsync(HttpContext context, IApiRequestLogRepository logRepository, IWebHostEnvironment environment)
  {
    var stopwatch = Stopwatch.StartNew();
    var now = DateTime.UtcNow;

    var requestBody = await ReadRequestBodyAsync(context.Request);
    var requestHeaders = MaskHeaders(context.Request.Headers);

    var originalBody = context.Response.Body;
    await using var responseBodyStream = new MemoryStream();
    context.Response.Body = responseBodyStream;

    Exception? capturedException = null;

    try
    {
      await _next(context);
    }
    catch (Exception ex)
    {
      capturedException = ex;
      throw;
    }
    finally
    {
      stopwatch.Stop();

      responseBodyStream.Seek(0, SeekOrigin.Begin);
      var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
      responseBodyStream.Seek(0, SeekOrigin.Begin);
      await responseBodyStream.CopyToAsync(originalBody);
      context.Response.Body = originalBody;

      var apiLog = new ApiRequestLog
      {
        DataHora = now,
        MetodoHttp = context.Request.Method,
        Endpoint = context.Request.Path.ToString(),
        QueryString = context.Request.QueryString.Value,
        RequestHeaders = requestHeaders,
        RequestBody = MaskContent(requestBody),
        StatusCode = context.Response.StatusCode,
        ResponseBody = MaskContent(responseBody),
        TempoExecucaoMs = stopwatch.ElapsedMilliseconds,
        UsuarioId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        UsuarioEmail = context.User.FindFirst(ClaimTypes.Email)?.Value,
        CorrelationId = GetCorrelationId(context),
        TraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier,
        IpOrigem = context.Connection.RemoteIpAddress?.ToString(),
        Sucesso = capturedException is null && context.Response.StatusCode is >= 200 and < 400,
        ExceptionType = capturedException?.GetType().FullName,
        ExceptionMessage = capturedException?.Message,
        StackTrace = Limit(capturedException?.StackTrace),
        Ambiente = environment.EnvironmentName
      };

      try
      {
        await logRepository.AddAsync(apiLog, CancellationToken.None);
      }
      catch (Exception ex)
      {
        _logger.LogError(
          ex,
          "Falha ao registrar log da request {Method} {Path} no banco de dados.",
          context.Request.Method,
          context.Request.Path);

        // Falhas no logging não devem interromper o fluxo principal.
      }
    }
  }

  private static async Task<string?> ReadRequestBodyAsync(HttpRequest request)
  {
    if (request.ContentLength is null or 0 || request.Body is null || !request.Body.CanRead)
      return null;

    request.EnableBuffering();
    request.Body.Position = 0;

    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    request.Body.Position = 0;

    return Limit(body);
  }

  private static string? GetCorrelationId(HttpContext context)
  {
    if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationHeader))
      return correlationHeader.ToString();

    if (context.Response.Headers.TryGetValue("X-Correlation-Id", out var responseCorrelationHeader))
      return responseCorrelationHeader.ToString();

    return context.TraceIdentifier;
  }

  private static string MaskHeaders(IHeaderDictionary headers)
  {
    var dictionary = headers.ToDictionary(
      x => x.Key,
      x => SensitiveFields.Contains(x.Key, StringComparer.OrdinalIgnoreCase)
        ? "***"
        : string.Join(",", x.Value.ToArray())
    );

    return Serialize(dictionary);
  }

  private static string? MaskContent(string? content)
  {
    if (string.IsNullOrWhiteSpace(content))
      return content;

    try
    {
      using var document = JsonDocument.Parse(content);
      var masked = MaskElement(document.RootElement);
      return Serialize(masked);
    }
    catch
    {
      return Limit(content);
    }
  }

  private static object? MaskElement(JsonElement element)
  {
    return element.ValueKind switch
    {
      JsonValueKind.Object => element
        .EnumerateObject()
        .ToDictionary(
          prop => prop.Name,
          prop => SensitiveFields.Contains(prop.Name, StringComparer.OrdinalIgnoreCase)
            ? "***"
            : MaskElement(prop.Value)
        ),
      JsonValueKind.Array => element.EnumerateArray().Select(MaskElement).ToList(),
      JsonValueKind.String => element.GetString(),
      JsonValueKind.Number => element.ToString(),
      JsonValueKind.True => true,
      JsonValueKind.False => false,
      _ => null
    };
  }

  private static string Serialize(object? value)
    => JsonSerializer.Serialize(value);

  private static string? Limit(string? value)
    => value is null ? null : value.Length <= MaxBodyLength ? value : value[..MaxBodyLength];
}
