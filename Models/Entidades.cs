using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoVentaApi.Models
{
    [Table("Puestos")]
    public class Puesto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_puesto")]
        public short IdPuesto { get; set; }

        [Column("puesto")]
        public string NombrePuesto { get; set; } = string.Empty;
    }

    [Table("Marcas")]
    public class Marca
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_marca")]
        public short IdMarca { get; set; }

        [Column("marca")]
        public string NombreMarca { get; set; } = string.Empty;
    }

    [Table("Proveedores")]
    public class Proveedor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Column("proveedor")]
        public string NombreProveedor { get; set; } = string.Empty;

        [Column("nit")]
        public string Nit { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }
    }

    [Table("Clientes")]
    public class Cliente
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("nit")]
        public string Nit { get; set; } = string.Empty;

        [Column("genero")]
        public bool Genero { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("correo_electronico")]
        public string? CorreoElectronico { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
    }

    [Table("Empleados")]
    public class Empleado
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [Column("nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Column("apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("dpi")]
        public string Dpi { get; set; } = string.Empty;

        [Column("genero")]
        public bool Genero { get; set; }

        [Column("fecha_nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Column("id_puesto")]
        public short IdPuesto { get; set; }

        [Column("fecha_inicio_labores")]
        public DateTime? FechaInicioLabores { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public Puesto? Puesto { get; set; }
    }

    [Table("Productos")]
    public class Producto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("producto")]
        public string NombreProducto { get; set; } = string.Empty;

        [Column("id_marca")]
        public short IdMarca { get; set; }

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("imagen")]
        public string? Imagen { get; set; }

        [Column("precio_costo", TypeName = "decimal(8,2)")]
        public decimal PrecioCosto { get; set; }

        [Column("precio_venta", TypeName = "decimal(8,2)")]
        public decimal PrecioVenta { get; set; }

        [Column("existencia")]
        public int Existencia { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public Marca? Marca { get; set; }
    }

    [Table("Compras")]
    public class Compra
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("no_orden_compra")]
        public int NoOrdenCompra { get; set; }

        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Column("fecha_orden")]
        public DateTime FechaOrden { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public Proveedor? Proveedor { get; set; }

        public List<CompraDetalle> Detalles { get; set; } = new();
    }

    [Table("Compras_detalle")]
    public class CompraDetalle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_compra_detalle")]
        public long IdCompraDetalle { get; set; }

        [Column("id_compra")]
        public int IdCompra { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_costo_unitario", TypeName = "decimal(8,2)")]
        public decimal PrecioCostoUnitario { get; set; }

        public Compra? Compra { get; set; }

        public Producto? Producto { get; set; }
    }

    [Table("Ventas")]
    public class Venta
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_venta")]
        public int IdVenta { get; set; }

        [Column("no_factura")]
        public int NoFactura { get; set; }

        [Column("serie")]
        public string Serie { get; set; } = string.Empty;

        [Column("fecha_factura")]
        public DateTime FechaFactura { get; set; }

        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("id_empleado")]
        public int IdEmpleado { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public Cliente? Cliente { get; set; }

        public Empleado? Empleado { get; set; }

        public List<VentaDetalle> Detalles { get; set; } = new();
    }

    [Table("Ventas_detalle")]
    public class VentaDetalle
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_venta_detalle")]
        public long IdVentaDetalle { get; set; }

        [Column("id_venta")]
        public int IdVenta { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario", TypeName = "decimal(8,2)")]
        public decimal PrecioUnitario { get; set; }

        public Venta? Venta { get; set; }

        public Producto? Producto { get; set; }
    }

    [Table("Usuarios")]
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("id_empleado")]
        public int? IdEmpleado { get; set; }

        [Column("fecha_ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;
    }

    [Table("Menu")]
    public class MenuOpcion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Column("id_menu")]
        public int IdMenu { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("url")]
        public string? Url { get; set; }

        [Column("icono")]
        public string? Icono { get; set; }

        [Column("id_padre")]
        public int? IdPadre { get; set; }

        [Column("orden")]
        public int Orden { get; set; }
    }
}
