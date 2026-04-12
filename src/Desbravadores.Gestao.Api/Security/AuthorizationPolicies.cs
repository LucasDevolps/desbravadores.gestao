using Desbravadores.Gestao.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Desbravadores.Gestao.Api.Security;

public static class AuthorizationPolicies
{
    public static AuthorizationBuilder AddPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy("MasterOnly", policy =>
            policy.RequireRole(
                Role.DIRETORIA.ToString(),
                Role.SECRETARIA.ToString()))

        .AddPolicy("Financeiro", policy =>
            policy.RequireRole(
                Role.TESOURARIA.ToString(),
                Role.DIRETORIA.ToString()));
        return builder;
    }
}