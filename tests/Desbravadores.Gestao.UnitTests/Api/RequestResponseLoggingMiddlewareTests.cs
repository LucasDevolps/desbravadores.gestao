using System.Text;
using Desbravadores.Gestao.Api.Middlewares;
using Desbravadores.Gestao.UnitTests.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Desbravadores.Gestao.UnitTests.Api;

public sealed class RequestResponseLoggingMiddlewareTests
{
  [Fact]
  public async Task InvokeAsync_logs_request_and_response_to_repository()
  {
    var repository = new FakeApiRequestLogRepository();
    var environment = new FakeWebHostEnvironment();
    var middleware = new RequestResponseLoggingMiddleware(
      async context =>
      {
        context.Response.StatusCode = StatusCodes.Status201Created;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"ok\":true}");
      },
      NullLogger<RequestResponseLoggingMiddleware>.Instance);
    var responseStream = new MemoryStream();
    var requestBody = Encoding.UTF8.GetBytes("{\"senha\":\"segredo\",\"nome\":\"Lucas\"}");
    var context = new DefaultHttpContext
    {
      Response =
      {
        Body = responseStream
      }
    };
    context.Request.Method = HttpMethods.Post;
    context.Request.Path = "/api/teste";
    context.Request.QueryString = new QueryString("?pagina=1");
    context.Request.Headers["X-Correlation-Id"] = "corr-1";
    context.Request.ContentLength = requestBody.Length;
    context.Request.Body = new MemoryStream(requestBody);

    await middleware.InvokeAsync(context, repository, environment);

    var log = Assert.Single(repository.Logs);
    Assert.Equal(HttpMethods.Post, log.MetodoHttp);
    Assert.Equal("/api/teste", log.Endpoint);
    Assert.Equal("?pagina=1", log.QueryString);
    Assert.Equal(StatusCodes.Status201Created, log.StatusCode);
    Assert.True(log.Sucesso);
    Assert.Equal("corr-1", log.CorrelationId);
    Assert.Equal(Environments.Development, log.Ambiente);
    Assert.Contains("\"senha\":\"***\"", log.RequestBody);
    Assert.Contains("\"nome\":\"Lucas\"", log.RequestBody);
    Assert.Equal("{\"ok\":true}", log.ResponseBody);

    responseStream.Position = 0;
    var response = await new StreamReader(responseStream).ReadToEndAsync();
    Assert.Equal("{\"ok\":true}", response);
  }

  [Fact]
  public async Task InvokeAsync_does_not_use_aborted_request_token_to_save_log()
  {
    var repository = new FakeApiRequestLogRepository();
    var environment = new FakeWebHostEnvironment();
    var middleware = new RequestResponseLoggingMiddleware(
      context =>
      {
        context.Response.StatusCode = StatusCodes.Status204NoContent;
        return Task.CompletedTask;
      },
      NullLogger<RequestResponseLoggingMiddleware>.Instance);
    using var abortedRequest = new CancellationTokenSource();
    await abortedRequest.CancelAsync();
    var context = new DefaultHttpContext
    {
      RequestAborted = abortedRequest.Token,
      Response =
      {
        Body = new MemoryStream()
      }
    };
    context.Request.Method = HttpMethods.Get;
    context.Request.Path = "/api/teste";

    await middleware.InvokeAsync(context, repository, environment);

    Assert.Single(repository.Logs);
    Assert.False(repository.LastCancellationToken.CanBeCanceled);
  }

  private sealed class FakeWebHostEnvironment : IWebHostEnvironment
  {
    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "Desbravadores.Gestao.UnitTests";
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
  }
}
