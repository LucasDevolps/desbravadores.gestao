using Desbravadores.Gestao.Application.Common;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;
using Desbravadores.Gestao.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController : BaseController
{

  [Authorize(Policy = "MasterOnly")]
  [HttpPost]
  [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> CriarUsuario(
      [FromServices] CriarUsuarioRequestHandler criarUsuarioHandler,
      [FromBody] CriarUsuarioRequest request,
      CancellationToken cancellationToken = default)
  {
    return await ExecuteCreatedAsync(
        request,
        criarUsuarioHandler,
        nameof(ObterPorId),
        id => new { id },
        id => new { id },
        cancellationToken);
  }
  [AllowAnonymous]
  [HttpPost("publicos")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> CriarUsuarioPublicos(
    [FromServices] CriarUsuarioRequestHandler criarUsuarioHandler,
    [FromBody] CriarUsuarioRequest request,
    CancellationToken cancellationToken = default)
  {
    return await ExecuteCreatedAsync(
        request,
        criarUsuarioHandler,
        nameof(ObterPorId),
        id => new { id },
        id => new { id },
        cancellationToken);
  }
  [Authorize(Policy = "Financeiro")]
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> GetAll(
    [FromServices] GetAllUsuariosRequestHandler getAllUsuariosRequestHandler,
    CancellationToken cancellationToken = default)
  {
    return await ExecuteAsync(new EmptyRequest(), getAllUsuariosRequestHandler, cancellationToken);
  }
  [Authorize(Policy = "MasterOnly")]
  [HttpGet("{id:guid}")]
  [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> ObterPorId(
    [FromServices] BuscaPorIdRequestHandler buscaPorIdHandler,
    [FromRoute] Guid id,    
    CancellationToken cancellationToken = default)
  {
    return await ExecuteAsync(
        id,
        buscaPorIdHandler,
        cancellationToken);
  }
}