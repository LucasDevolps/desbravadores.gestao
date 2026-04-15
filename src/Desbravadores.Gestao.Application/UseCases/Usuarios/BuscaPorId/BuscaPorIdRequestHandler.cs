using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;

public class BuscaPorIdRequestHandler(IUsuarioRepository usuarioRepository) : IRequestHandler<BuscaUsuarioPorIdQuery, UsuarioDTO>
{
  private readonly IUsuarioRepository usuarioRepository = usuarioRepository;

  public async Task<UsuarioDTO> Handle(BuscaUsuarioPorIdQuery query, CancellationToken cancellationToken = default)
  {
    return await usuarioRepository.GetByIdAsync(query.Id, cancellationToken) 
      ?? throw new KeyNotFoundException("Usuário não encontrado.");
  }
}
