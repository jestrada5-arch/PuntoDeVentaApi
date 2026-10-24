using Microsoft.EntityFrameworkCore;
using PuntoVentaApi.Models;

namespace PuntoVentaApi.Data
{
    public class PuntoVentaContext : DbContext
    {
        public PuntoVentaContext(DbContextOptions<PuntoVentaContext> options) : base(options)
        {
        }

        public DbSet<Puesto> Puestos => Set<Puesto>();
        public DbSet<Marca> Marcas => Set<Marca>();
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Empleado> Empleados => Set<Empleado>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Compra> Compras => Set<Compra>();
        public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();
        public DbSet<Venta> Ventas => Set<Venta>();
        public DbSet<VentaDetalle> VentasDetalle => Set<VentaDetalle>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<MenuOpcion> Menu => Set<MenuOpcion>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Puesto).WithMany().HasForeignKey(e => e.IdPuesto);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Marca).WithMany().HasForeignKey(p => p.IdMarca);

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor).WithMany().HasForeignKey(c => c.IdProveedor);

            modelBuilder.Entity<CompraDetalle>()
                .HasOne(d => d.Compra).WithMany(c => c.Detalles)
                .HasForeignKey(d => d.IdCompra).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CompraDetalle>()
                .HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.IdProducto);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.IdCliente);

            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Empleado).WithMany().HasForeignKey(v => v.IdEmpleado);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(d => d.Venta).WithMany(v => v.Detalles)
                .HasForeignKey(d => d.IdVenta).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VentaDetalle>()
                .HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.IdProducto);
        }
    }
}
