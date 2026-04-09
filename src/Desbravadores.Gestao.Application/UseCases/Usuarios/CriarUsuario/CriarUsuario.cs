using Desbravadores.Gestao.Domain.Constants;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed record CriarUsuario(
    string Nome,
    string Email,
    string Senha,
    string Roles
);