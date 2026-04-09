using Desbravadores.Gestao.Application.Auth.Login;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(LoginHandler loginHandler, IValidator<LoginRequest> validator) : ControllerBase
{
  private readonly LoginHandler _loginHandler = loginHandler;
  private readonly IValidator<LoginRequest> _validator = validator;

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromBody] LoginRequest request,
      CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      return BadRequest(validationResult.Errors);
    }

    var response = await _loginHandler.HandleAsync(request, cancellationToken);
    return Ok(response);
  }
}