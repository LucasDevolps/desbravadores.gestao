using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;

public sealed record DeletarUsuarioCommand(Guid Id) : IRequest;
