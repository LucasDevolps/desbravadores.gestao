using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Desbravadores.Gestao.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IValidator<LoginRequest> validator) : ControllerBase
{
  private readonly IValidator<LoginRequest> _validator = validator;

  [HttpPost("login")]
  [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> Login(
      [FromServices] LoginRequestHandler loginRequestHandler,
      [FromBody] LoginRequest request,
      CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
    {
      return BadRequest(validationResult.Errors);
    }

    var response = await loginRequestHandler.HandleAsync(request, cancellationToken);
    return Ok(response);
  }
  [Authorize]
  [HttpPost("logout")]
  public async Task<IActionResult> Logout(
    [FromServices] IUsuarioSessaoRepository usuarioSessaoRepository,
    CancellationToken cancellationToken)
  {
    var jti = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

    if (string.IsNullOrWhiteSpace(jti))
      return Unauthorized();

    var sessao = await usuarioSessaoRepository.GetByJtiAsync(jti, cancellationToken);

    if (sessao is null)
      return Unauthorized();

    sessao.Revogar();
    await usuarioSessaoRepository.SaveChangesAsync(cancellationToken);

    return NoContent();
  }
  [Authorize]
  [HttpGet("me")]
  public async Task<IActionResult> Me(
  [FromServices] IUsuarioRepository usuarioRepository,
  CancellationToken cancellationToken)
  {
    var uuidValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!Guid.TryParse(uuidValue, out var uuid))
      return Unauthorized("Token inválido: UUID não encontrado.");

    var usuario = await usuarioRepository.GetByUuidAsync(uuid, cancellationToken);

    if (usuario is null)
      return NotFound("Usuário não encontrado.");

    return Ok(usuario);
  }
}