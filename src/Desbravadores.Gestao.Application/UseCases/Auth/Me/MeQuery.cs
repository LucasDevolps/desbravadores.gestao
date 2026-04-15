using Desbravadores.Gestao.Domain.DTOs;
using MediatR;

namespace Desbravadores.Gestao.Application.UseCases.Auth.Me;

public sealed record MeQuery(string Sub, string Jti) : IRequest<UsuarioDTO>;