using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController(IValidator<CriarUsuarioRequest> validator) : ControllerBase
{
  private readonly IValidator<CriarUsuarioRequest> _validator = validator;

  [Authorize(Policy = "MasterOnly")]
  [HttpPost]
  [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> CriarUsuario(
    [FromServices] CriarUsuarioRequestHandler criarUsuarioHandler,
    [FromBody] CriarUsuarioRequest request,
    CancellationToken cancellationToken = default)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
      return BadRequest(validationResult.Errors);

    var id = await criarUsuarioHandler.HandleAsync(request, cancellationToken);

    return CreatedAtAction(
      nameof(ObterPorId),
      new { id },
      new { id });
  }

  [Authorize(Policy = "Financeiro")]
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> GetAll(
    [FromServices] IUsuarioRepository usuarioRepository,
    CancellationToken cancellationToken = default)
  {
    var usuarios = await usuarioRepository.GetAllAsync(cancellationToken);
    return Ok(usuarios);
  }

  [Authorize(Policy = "MasterOnly")]
  [HttpGet("{id:guid}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ObterPorId(
    [FromRoute] Guid id,
    [FromServices] IUsuarioRepository usuarioRepository,
    CancellationToken cancellationToken = default)
  {
    var usuario = await usuarioRepository.GetByIdAsync(id, cancellationToken);

    if (usuario is null)
      return NotFound("Usuário não encontrado.");

    return Ok(usuario);
  }
}