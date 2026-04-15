using Desbravadores.Gestao.Application.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;


[ApiController]
public abstract class BaseController : ControllerBase
{
  protected async Task<IActionResult> ExecuteAsync<TRequest, TResponse>(
      TRequest request,
      IAppRequestHandler<TRequest, TResponse> handler,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var validator = HttpContext.RequestServices.GetService<IValidator<TRequest>>();

      if (validator is not null)
      {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
          return BadRequest(new
          {
            Mensagem = "Foram encontrados erros de validação.",
          });
        }
      }

      var response = await handler.HandleAsync(request, cancellationToken);

      return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { Mensagem = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { Mensagem = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { Mensagem = ex.Message });
    }
    catch (InvalidCastException ex)
    {
      return BadRequest(new { Mensagem = ex.Message });
    }
    catch 
    {
      return StatusCode(StatusCodes.Status500InternalServerError, new
      {
        Mensagem = "Ocorreu um erro interno ao processar a requisição."
      });
    }
  }

  protected async Task<IActionResult> ExecuteCreatedAsync<TRequest, TResponse>(
      TRequest request,
      IAppRequestHandler<TRequest, TResponse> handler,
      string actionName,
      Func<TResponse, object> routeValues,
      Func<TResponse, object> responseBody,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var validator = HttpContext.RequestServices.GetService<IValidator<TRequest>>();

      if (validator is not null)
      {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
          return BadRequest(new
          {
            Mensagem = "Ocorreu um erro interno ao processar a requisição."
          });
        }
      }

      var response = await handler.HandleAsync(request, cancellationToken);

      return CreatedAtAction(
          actionName,
          routeValues(response),
          responseBody(response));
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { Mensagem = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { Mensagem = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { Mensagem = ex.Message });
    }
    catch (InvalidCastException ex)
    {
      return BadRequest(new { Mensagem = ex.Message });
    }
    catch 
    {
      return StatusCode(StatusCodes.Status500InternalServerError, new
      {
        Mensagem = "Ocorreu um erro interno ao processar a requisição.",
      });
    }
  }
  protected async Task<IActionResult> ExecuteNoContentAsync<TRequest>(
    TRequest request,
    IAppRequestHandler<TRequest, Unit> handler,
    CancellationToken cancellationToken = default)
  {
    try
    {
      var validator = HttpContext.RequestServices.GetService<IValidator<TRequest>>();

      if (validator is not null)
      {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
          return BadRequest(new
          {
            Mensagem = "Erros de validação",
          });
        }
      }

      await handler.HandleAsync(request, cancellationToken);

      return NoContent();
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { Mensagem = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { Mensagem = ex.Message });
    }
    catch
    {
      return StatusCode(500, new
      {
        Mensagem = "Ocorreu um erro interno ao processar a requisição.",
      });
    }
  }
}