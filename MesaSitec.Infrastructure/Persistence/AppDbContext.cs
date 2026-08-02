using Microsoft.EntityFrameworkCore;
using MesaSitec.Domain.Entities;

namespace MesaSitec.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Solicitud> Solicitudes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Solicitud>(entity =>
        {
            entity.HasIndex(s => new { s.TenantId, s.Codigo }).IsUnique();
            entity.Property(s => s.Titulo).HasMaxLength(120);
            entity.Property(s => s.Descripcion).HasMaxLength(4000);
            entity.Property(s => s.Codigo).HasMaxLength(20);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(100);
        });
    }
}