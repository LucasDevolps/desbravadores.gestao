using Desbravadores.Gestao.Application.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed record AtualizarUsuarioCommand(
    Guid Uuid,
    string? Nome,
    string? Email,
    string? Senha,
    string? Roles,
    string? IpUsuarioLogado = null,
    Guid? UsuarioLogado = null
) : IRequest<UsuarioDTO>;
