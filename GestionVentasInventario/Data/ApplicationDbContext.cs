using GestionVentasInventario.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestionVentasInventario.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración Producto
            builder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(18,2)");

            // Configuración Venta
            builder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("decimal(18,2)");

            // Configuración DetalleVenta
            builder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("decimal(18,2)");

            builder.Entity<DetalleVenta>()
                .Property(d => d.Subtotal)
                .HasColumnType("decimal(18,2)");

            // Seed de Categorías iniciales
            builder.Entity<Categoria>().HasData(
                new Categoria { ID = 1, Nombre = "Electrónica", Descripcion = "Productos electrónicos", Estado = true },
                new Categoria { ID = 2, Nombre = "Ropa", Descripcion = "Prendas de vestir", Estado = true },
                new Categoria { ID = 3, Nombre = "Alimentos", Descripcion = "Productos alimenticios", Estado = true }
            );
        }
    }
}