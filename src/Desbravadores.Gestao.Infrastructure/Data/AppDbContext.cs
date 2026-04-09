using Desbravadores.Gestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Data
{
  public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
  {
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioSessao> UsuarioSessoes => Set<UsuarioSessao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Usuario>(entity =>
      {
        entity.ToTable("Usuarios");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.Uuid)
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

        entity.HasIndex(x => x.Email)
              .IsUnique();
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
