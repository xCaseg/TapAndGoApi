using Microsoft.EntityFrameworkCore;
using TapAndGo.Api.Models;

namespace TapAndGo.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MenuItem>().Property(p => p.PrecioChico).HasPrecision(10, 2);
            modelBuilder.Entity<MenuItem>().Property(p => p.PrecioMediano).HasPrecision(10, 2);
            modelBuilder.Entity<MenuItem>().Property(p => p.PrecioGrande).HasPrecision(10, 2);
            modelBuilder.Entity<MenuItem>().Property(p => p.Stock).HasPrecision(10, 2);
            modelBuilder.Entity<MenuItem>().Property(p => p.Calorias).HasPrecision(10, 2);

            modelBuilder.Entity<Pedido>().Property(p => p.Total).HasPrecision(10, 2);

            modelBuilder.Entity<Cliente>()
            .HasMany(c => c.Pedidos)
            .WithOne(p => p.Cliente)
            .HasForeignKey(p => p.ClienteId);
        }
    }
}
