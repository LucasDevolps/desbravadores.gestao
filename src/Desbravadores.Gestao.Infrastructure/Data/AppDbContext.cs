using Desbravadores.Gestao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desbravadores.Gestao.Infrastructure.Data
{
  public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
  {
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Usuario>(entity =>
      {
        entity.ToTable("Usuarios");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => x.Uuid)
              .HasName("UuidIndex")
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
    }
  }
}
