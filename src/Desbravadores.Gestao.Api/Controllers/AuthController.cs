using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.Auth.Me;
using Desbravadores.Gestao.Domain.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IValidator<LoginRequest> validator) : ControllerBase
{
  private readonly IValidator<LoginRequest> _validator = validator;

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromServices] LoginRequestHandler loginRequestHandler,
      [FromBody] LoginRequest request,
      CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
      return BadRequest(validationResult.Errors);

    var response = await loginRequestHandler.HandleAsync(request, cancellationToken);
    return Ok(response);
  }

  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Logout(
    [FromServices] LogoutRequestHandler logoutRequestHandler,
    CancellationToken cancellationToken)
  {
    await logoutRequestHandler.HandleAsync(
        User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty,
        cancellationToken);

    return NoContent();
  }

  [Authorize]
  [HttpGet("Me")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Me(
    [FromServices] MeRequestHandler meRequestHandler,
    CancellationToken cancellationToken)
  {
    var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

    if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(jti))
      return Unauthorized("Token inválido.");

    var usuario = await meRequestHandler.HandleAsync(sub, jti, cancellationToken);

    return Ok(usuario);
  }
}