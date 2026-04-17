using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : Controller
{
  private readonly IMediator _mediator = mediator;

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromBody] LoginCommand command,
      CancellationToken cancellationToken)
  {
    var response = await _mediator.Send(command, cancellationToken);
    return Ok(response);
  }

  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Logout(
    CancellationToken cancellationToken)
  {
    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;

    await _mediator.Send(new LogoutCommand(jti), cancellationToken);

    return NoContent();
  }

  [Authorize]
  [HttpGet("Me")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Me(
    CancellationToken cancellationToken)
  {
    var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
              ?? string.Empty;

    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
              ?? string.Empty;

    var response = await _mediator.Send(new MeQuery(sub, jti), cancellationToken);

    return Ok(response);
  }
}