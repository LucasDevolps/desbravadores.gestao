using Desbravadores.Gestao.Application.Interfaces;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.DeletarUsuario;

public sealed class DeletarUsuarioCommandHandler(IUsuarioRepository usuarioRepository) : IRequestHandler<DeletarUsuarioCommand>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    public async Task Handle(DeletarUsuarioCommand request, CancellationToken cancellationToken)
    {
        await _usuarioRepository.DeletarUsuarioAsync(request.Id, cancellationToken);
    }
}
