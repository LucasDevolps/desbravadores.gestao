namespace Desbravadores.Gestao.Application.UseCases.Usuarios.CriarUsuario;

public sealed record CriarUsuario(
    string Nome,
    string Email,
    string Senha
);