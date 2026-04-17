using Desbravadores.Gestao.Domain.DTOs;
using Desbravadores.Gestao.Domain.Entities;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.AtualizarUsuario;

public sealed record AtualizarUsuarioCommand(Usuario command) : IRequest<UsuarioDTO>;
