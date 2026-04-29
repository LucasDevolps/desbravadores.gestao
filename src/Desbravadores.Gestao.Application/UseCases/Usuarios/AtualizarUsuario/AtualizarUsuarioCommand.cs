using Desbravadores.Gestao.Application.DTOs;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed record AtualizarUsuarioCommand(Usuario Command) : IRequest<UsuarioDTO>;
