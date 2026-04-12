using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Desbravadores.Gestao.Api.Security;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                     ?? throw new InvalidOperationException("JWT_KEY não configurado.");

        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                        ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                          ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");

        services
          .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = jwtIssuer,
              ValidAudience = jwtAudience,
              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
          });

        return services;
    }
}
