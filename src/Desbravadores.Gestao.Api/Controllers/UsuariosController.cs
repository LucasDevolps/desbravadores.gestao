using Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using Desbravadores.Gestao.Domain.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController(IMediator mediator) : Controller
{

  private readonly IMediator _mediator = mediator;

  [Authorize(Policy = "MasterOnly")]
  [HttpPost]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> CriarUsuario(
        [FromBody] CriarUsuarioCommand command,
        CancellationToken cancellationToken = default)
  {
    var id = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
  }

  [AllowAnonymous]
  [HttpPost("publicos")]
  [Consumes("application/json")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> CriarUsuarioPublicos(
        [FromBody] CriarUsuarioCommand command,
        CancellationToken cancellationToken = default)
  {
    var id = await _mediator.Send(command, cancellationToken);
    return CreatedAtAction(nameof(ObterPorId), new { id }, new { id });
  }

  [Authorize(Policy = "Financeiro")]
  [HttpGet]
  [ProducesResponseType(typeof(IEnumerable<UsuarioDTO>), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
  {
    var response = await _mediator.Send(new GetAllUsuariosQuery(), cancellationToken);
    return Ok(response);
  }

  [Authorize(Policy = "MasterOnly")]
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ObterPorId(
    [FromRoute] Guid id,    
    CancellationToken cancellationToken = default)
  {
    var query = new BuscaUsuarioPorIdQuery(id);
    var response = await _mediator.Send(query, cancellationToken);
    return Ok(response);
  }
  [Authorize(Policy = "MasterOnly")]
  [HttpDelete("{id:guid}")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> DeletarUsuario(
    [FromRoute] Guid id,
    CancellationToken cancellationToken = default)
  {
    var command = new DeletarUsuarioCommand(id);
    await _mediator.Send(command, cancellationToken);
    return NoContent();
  }
  [Authorize(Policy = "MasterOnly")]
  [HttpPut]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> AtualizarUsuario(
    [FromBody] AtualizarUsuarioCommand command,
    CancellationToken cancellationToken = default)
  {
    var response = await _mediator.Send(command, cancellationToken);
    return Ok(response);
  }


}