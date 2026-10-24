using System.ComponentModel.DataAnnotations;

namespace PuntoVentaApi.Models
{
    // ---------- Proveedores ----------
    public class ProveedorDto
    {
        public int IdProveedor { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
        [StringLength(100, ErrorMessage = "El proveedor admite máximo 100 caracteres.")]
        public string Proveedor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es obligatorio.")]
        [StringLength(12, ErrorMessage = "El NIT admite máximo 12 caracteres.")]
        [RegularExpression(@"^\d{1,10}-?[0-9Kk]\z", ErrorMessage = "NIT inválido (ejemplo: 123456-7).")]
        public string Nit { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "La dirección admite máximo 150 caracteres.")]
        public string? Direccion { get; set; }

        [RegularExpression(@"^\d{8,12}\z", ErrorMessage = "El teléfono debe tener entre 8 y 12 dígitos.")]
        public string? Telefono { get; set; }
    }

    // ---------- Empleados ----------
    public class EmpleadoDto : IValidatableObject
    {
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(60, ErrorMessage = "Los nombres admiten máximo 60 caracteres.")]
        [RegularExpression(@"^\p{L}[\p{L} .'\-]*\z", ErrorMessage = "Los nombres solo admiten letras.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(60, ErrorMessage = "Los apellidos admiten máximo 60 caracteres.")]
        [RegularExpression(@"^\p{L}[\p{L} .'\-]*\z", ErrorMessage = "Los apellidos solo admiten letras.")]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "La dirección admite máximo 150 caracteres.")]
        public string? Direccion { get; set; }

        [RegularExpression(@"^\d{8,12}\z", ErrorMessage = "El teléfono debe tener entre 8 y 12 dígitos.")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El DPI es obligatorio.")]
        [RegularExpression(@"^\d{13}\z", ErrorMessage = "El DPI debe tener 13 dígitos.")]
        public string Dpi { get; set; } = string.Empty;

        // true = masculino, false = femenino
        public bool Genero { get; set; } = true;

        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El puesto es obligatorio.")]
        public short IdPuesto { get; set; }

        public string? Puesto { get; set; }

        public DateTime? FechaInicioLabores { get; set; }

        public DateTime FechaIngreso { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaNacimiento.HasValue)
            {
                var edad = DateTime.Today.Year - FechaNacimiento.Value.Year;
                if (FechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad)) edad--;

                if (edad < 18)
                {
                    yield return new ValidationResult(
                        "El empleado debe ser mayor de edad.",
                        new[] { nameof(FechaNacimiento) });
                }

                if (FechaNacimiento.Value > DateTime.Today)
                {
                    yield return new ValidationResult(
                        "La fecha de nacimiento no es válida.",
                        new[] { nameof(FechaNacimiento) });
                }
            }
        }
    }

    // ---------- Productos ----------
    public class ProductoDto
    {
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100, ErrorMessage = "El producto admite máximo 100 caracteres.")]
        public string Producto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public short IdMarca { get; set; }

        public string? Marca { get; set; }

        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string? Descripcion { get; set; }

        public string? Imagen { get; set; }

        [Required(ErrorMessage = "El precio de costo es obligatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio de costo debe ser mayor a cero.")]
        public decimal PrecioCosto { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "El precio de venta debe ser mayor a cero.")]
        public decimal PrecioVenta { get; set; }

        public int Existencia { get; set; }

        public DateTime FechaIngreso { get; set; }
    }
}
