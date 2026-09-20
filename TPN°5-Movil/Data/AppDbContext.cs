using Microsoft.EntityFrameworkCore;
using TPN_5_MOVIL.Entities;

namespace TPN_5_MOVIL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<Salida> Salidas => Set<Salida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(e =>
        {
            e.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            e.HasIndex(c => c.Nombre).IsUnique();
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            e.Property(p => p.CUIT).IsRequired().HasMaxLength(20);
            e.HasIndex(p => p.CUIT).IsUnique();
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nombre).IsRequired().HasMaxLength(150);
            e.Property(c => c.CUIT).IsRequired().HasMaxLength(20);
            e.HasIndex(c => c.CUIT).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(150);
            e.Property(u => u.Email).IsRequired().HasMaxLength(150);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Producto>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
            e.Property(p => p.Precio).HasPrecision(18, 2);
            e.Property(p => p.ImagenUrl).HasMaxLength(300);

            e.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(p => p.CategoriaId);
            e.HasIndex(p => p.ProveedorId);
            e.HasIndex(p => p.Nombre);
        });

        modelBuilder.Entity<Ingreso>(e =>
        {
            e.Property(i => i.Cantidad).IsRequired();

            e.HasOne(i => i.Producto)
                .WithMany(p => p.Ingresos)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Proveedor)
                .WithMany(pr => pr.Ingresos)
                .HasForeignKey(i => i.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(i => i.ProductoId);
            e.HasIndex(i => i.ProveedorId);
            e.HasIndex(i => i.Fecha);
        });

        modelBuilder.Entity<Salida>(e =>
        {
            e.Property(s => s.Cantidad).IsRequired();
            e.Property(s => s.PrecioUnitario).HasPrecision(18, 2);

            e.HasOne(s => s.Producto)
                .WithMany(p => p.Salidas)
                .HasForeignKey(s => s.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Cliente)
                .WithMany(c => c.Salidas)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => s.ProductoId);
            e.HasIndex(s => s.ClienteId);
            e.HasIndex(s => s.Fecha);
        });
    }
}