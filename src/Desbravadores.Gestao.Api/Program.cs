using Desbravadores.Gestao.Api.Security;
using Desbravadores.Gestao.Application;
using Desbravadores.Gestao.Application.Auth.Login;
using Desbravadores.Gestao.Application.Interfaces;
using Desbravadores.Gestao.Domain.Interfaces.Repositories;
using Desbravadores.Gestao.Infrastructure;
using Desbravadores.Gestao.Infrastructure.Data;
using Desbravadores.Gestao.Infrastructure.Repositories;
using Desbravadores.Gestao.Infrastructure.Security;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.Converters.Add(
          new System.Text.Json.Serialization.JsonStringEnumConverter()
      );
    });
    
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUsuarioSessaoRepository, UsuarioSessaoRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<LoginRequestHandler>();
builder.Services.AddScoped<LoginRequestValidator>();


builder.Services.AddJwtAuthentication();

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

builder.Services.AddAuthorizationBuilder().AddPolicies();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();