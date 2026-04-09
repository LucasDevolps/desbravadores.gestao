using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Infrastructure;
using Desbravadores.Gestao.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<LoginHandler>();
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
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
      };
    });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
