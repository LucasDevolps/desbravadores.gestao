using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace Desbravadores.Gestao.Api.Security;

public static class JwtAuthenticationExtensions
{
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
  {
    string jwtKey = GetJWT_KEY();

    var jwtIssuer = GetJWT_ISSUER();

    var jwtAudience = GetJWT_AUDIENCE();

    services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
          };

            options.Events = new JwtBearerEvents
            {
              OnTokenValidated = async context =>
              {
                var uuidValue = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!Guid.TryParse(uuidValue, out var uuid) || string.IsNullOrWhiteSpace(jti))
                {
                  context.Fail("Token inválido.");
                  return;
                }

                var usuarioRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IUsuarioRepository>();

                var usuarioSessaoRepository = context.HttpContext.RequestServices
                    .GetRequiredService<IUsuarioSessaoRepository>();

                var usuario = await usuarioRepository.GetByUuidAsync(uuid, context.HttpContext.RequestAborted);

                if (usuario is null)
                {
                  context.Fail("Usuário não encontrado.");
                  return;
                }

                var tokenValido = await usuarioSessaoRepository.ExistsActiveSessionAsync(
                    usuario.Id,
                    jti,
                    context.HttpContext.RequestAborted);

                if (!tokenValido)
                  context.Fail("Token revogado, expirado ou inválido.");
              }
            };
          });

    return services;
  }
  public static string GetJWT_KEY()
  {
    return Environment.GetEnvironmentVariable("JWT_KEY")
        ?? throw new InvalidOperationException("JWT_KEY não configurado.");
  }
  public static string GetJWT_AUDIENCE()
  {
    return Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");
  }
  public static string GetJWT_ISSUER()
  {
    return Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");
  }
}
