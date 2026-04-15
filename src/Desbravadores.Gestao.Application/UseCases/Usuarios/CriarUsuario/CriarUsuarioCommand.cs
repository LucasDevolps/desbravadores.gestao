using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed record CriarUsuarioCommand(
    string Nome,
    string Email,
    string Senha,
    string Roles
) : IRequest<Guid>;