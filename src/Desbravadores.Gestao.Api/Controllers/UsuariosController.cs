using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;
using Desbravadores.Gestao.Domain.Constants;
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
  [HttpPost("CriarUsuario")]
  public async Task<IActionResult> CriarUsuario(
    [FromServices] CriarUsuarioRequestHandler criarUsuarioHandler,
    [FromBody] CriarUsuarioRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);

    if (!validationResult.IsValid)
      return BadRequest(validationResult.Errors);

    var id = await criarUsuarioHandler.HandleAsync(
      new CriarUsuarioRequest
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
