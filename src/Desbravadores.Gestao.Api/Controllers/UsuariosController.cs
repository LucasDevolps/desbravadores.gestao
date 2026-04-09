using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Desbravadores.Gestao.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsuariosController : ControllerBase
{
  [Authorize(Policy = "MasterOnly")]
  [HttpPost("CriarUsuario")]
  public async Task<IActionResult> CriarUsuario(
    [FromServices] CriarUsuarioHandler criarUsuarioHandler,
    [FromBody] CriarUsuario request,
    CancellationToken cancellationToken = default
  )
  {
    if (!Roles.Validas.Contains(request.Roles.Trim()))
      return BadRequest("Role inválida.");

    var id = await criarUsuarioHandler.HandleAsync(
      new CriarUsuario
      (
        request.Nome,
        request.Email,
        request.Senha,
        request.Roles
      ),
      cancellationToken
    );

    return CreatedAtAction(nameof(CriarUsuario), id);
  }

  [Authorize(Policy = "Financeiro")]
  [HttpGet("GetAll")]
  public async Task<IActionResult> GetAll(
    [FromServices] IUsuarioRepository usuarioRepository,
    CancellationToken cancellationToken = default
  )
  {
    var usuarios = await usuarioRepository.GetAllAsync(cancellationToken);
    return Ok(usuarios);
  }
}
