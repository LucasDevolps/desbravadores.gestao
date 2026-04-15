using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Usuarios.BuscaPorId;

public sealed class BuscaUsuarioPorIdQuery : IRequest<UsuarioDTO>
{
  public Guid Id { get; set; }
  public BuscaUsuarioPorIdQuery()
  {

  }
  public BuscaUsuarioPorIdQuery(Guid id)
  {
    Id = id;
  }
  public void RecuperarId(Guid id)
  {
    Id = id;
  }
}