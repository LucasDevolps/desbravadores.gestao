using Desbravadores.Gestao.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Desbravadores.Gestao.Api.Security;

public static class AuthorizationPolicies
{
    public static AuthorizationBuilder AddPolicies(this AuthorizationBuilder builder)
    {
        builder.AddPolicy("MasterOnly", policy =>
            policy.RequireRole(
                Roles.DIRETORIA.ToString(),
                Roles.SECRETARIA.ToString()))

        .AddPolicy("Financeiro", policy =>
            policy.RequireRole(
                Roles.TESOURARIA.ToString(),
                Roles.DIRETORIA.ToString()));
        return builder;
    }
}