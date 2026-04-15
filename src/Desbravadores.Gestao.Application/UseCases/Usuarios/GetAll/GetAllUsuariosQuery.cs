using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.GetAll;

public sealed record GetAllUsuariosQuery : IRequest<IEnumerable<UsuarioDTO>>;
