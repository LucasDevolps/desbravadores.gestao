using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Auth.Logout;
using Desbravadores.Gestao.Application.UseCases.Auth.Me;
using Desbravadores.Gestao.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromServices] LoginRequestHandler loginRequestHandler,
      [FromBody] LoginRequest request,
      CancellationToken cancellationToken)
  {
    return await ExecuteAsync(
        request,
        loginRequestHandler,
        cancellationToken);
  }

  [Authorize]
  [HttpPost("logout")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Logout(
    [FromServices] LogoutRequestHandler handler,
    CancellationToken cancellationToken)
  {
    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;

    return await ExecuteNoContentAsync(jti, handler, cancellationToken);
  }

  [Authorize]
  [HttpGet("Me")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Me(
    [FromServices] MeRequestHandler handler,
    CancellationToken cancellationToken)
  {
    var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

    if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(jti))
      return Unauthorized("Token inválido.");

    var request = new MeRequest(sub, jti);

    return await ExecuteAsync(request, handler, cancellationToken);
  }
}