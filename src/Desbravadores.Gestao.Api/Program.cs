using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Constants;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using Desbravadores.Gestao.Infrastructure;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Desbravadores.Gestao.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUsuarioSessaoRepository, UsuarioSessaoRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<LoginRequestHandler>();
builder.Services.AddScoped<LoginRequestValidator>();

var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
             ?? throw new InvalidOperationException("JWT_KEY não configurado.");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                  ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");

builder.Services
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
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
  });

builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Desbravadores.Gestao.Api",
    Version = "v1"
  });

  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Informe o token JWT no formato: Bearer {seu_token}"
  });

  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id = "Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

builder.Services.AddAuthorizationBuilder()
  .AddPolicy("MasterOnly", policy =>
      policy.RequireRole(
          Role.DIRETORIA.ToString(),
          Role.SECRETARIA.ToString()))
  .AddPolicy("Financeiro", policy =>
      policy.RequireRole(
          Role.TESOURARIA.ToString(),
          Role.DIRETORIA.ToString()));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();