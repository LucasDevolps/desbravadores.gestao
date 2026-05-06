using Desbravadores.Gestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Data
{
  public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
  {
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSessao> UsuarioSessoes => Set<UsuarioSessao>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Usuario>(entity =>
      {
        entity.ToTable("Usuarios");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.Uuid)
              .IsUnique();

        entity.HasIndex(x => x.Email)
              .IsUnique();

        entity.Property(x => x.Id)
              .ValueGeneratedOnAdd();

        entity.Property(x => x.Uuid)
              .IsRequired();

        entity.Property(x => x.Nome)
              .IsRequired()
              .HasMaxLength(150);

        entity.Property(x => x.Email)
              .IsRequired()
              .HasMaxLength(200);

        entity.Property(x => x.Senha)
              .IsRequired();

        entity.Property(x => x.DataCriacao)
              .IsRequired();

        entity.Property(x => x.DataAtualizacao)
              .IsRequired(false);

        entity.Property(x => x.UsuarioLogadoId)
              .HasColumnName("UsuarioLogado")
              .IsRequired(false);

        entity.Property(x => x.IpUsuarioLogado)
              .HasMaxLength(45)
              .IsRequired(false);

        entity
              .Property(x => x.Role)
              .HasConversion<string>();

        entity.HasOne(x => x.UsuarioLogado)
              .WithMany()
              .HasForeignKey(x => x.UsuarioLogadoId)
              .OnDelete(DeleteBehavior.Restrict);
      });


      modelBuilder.Entity<ApiRequestLog>(entity =>
      {
        entity.ToTable("ApiRequestLogs");

        entity.HasKey(x => x.Id);

        entity.Property(x => x.DataHora).IsRequired();
        entity.Property(x => x.MetodoHttp).IsRequired().HasMaxLength(12);
        entity.Property(x => x.Endpoint).IsRequired().HasMaxLength(500);
        entity.Property(x => x.StatusCode).IsRequired();
        entity.Property(x => x.TempoExecucaoMs).IsRequired();
        entity.Property(x => x.Sucesso).IsRequired();
        entity.Property(x => x.CorrelationId).HasMaxLength(120);
        entity.Property(x => x.TraceId).HasMaxLength(120);
        entity.Property(x => x.IpOrigem).HasMaxLength(45);
        entity.Property(x => x.UsuarioId).HasMaxLength(120);
        entity.Property(x => x.UsuarioEmail).HasMaxLength(200);
        entity.Property(x => x.Ambiente).HasMaxLength(64);

        entity.HasIndex(x => x.DataHora);
        entity.HasIndex(x => x.Endpoint);
        entity.HasIndex(x => x.StatusCode);
        entity.HasIndex(x => x.CorrelationId);
        entity.HasIndex(x => x.TraceId);
      });

      modelBuilder.Entity<UsuarioSessao>(entity =>
      {
        entity.ToTable("UsuarioSessoes");

        entity.HasKey(x => x.Id);

        entity.HasAlternateKey(x => x.Uuid);

        entity.Property(x => x.Jti)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(x => x.RefreshToken)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(x => x.AccessTokenExpiraEm)
            .IsRequired();

        entity.Property(x => x.RefreshTokenExpiraEm)
            .IsRequired();

        entity.Property(x => x.Revogado)
            .IsRequired();

        entity.Property(x => x.DataCriacao)
            .IsRequired();

        entity.HasIndex(x => x.Jti)
            .IsUnique();

        entity.HasIndex(x => x.RefreshToken)
            .IsUnique();

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.Sessoes)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
      });
    }
  }
}
