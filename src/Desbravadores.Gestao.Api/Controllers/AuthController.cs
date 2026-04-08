using Desbravadores.Gestao.Application.Auth.Login;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(LoginHandler loginHandler) : ControllerBase
{
  private readonly LoginHandler _loginHandler = loginHandler;

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromBody] LoginRequest request,
      CancellationToken cancellationToken)
  {
    var response = await _loginHandler.HandleAsync(request, cancellationToken);
    return Ok(response);
  }
}