using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVentaApi.Data;
using PuntoVentaApi.Models;

namespace PuntoVentaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        // Margen aplicado cuando no se indica un precio de venta manual.
        // Compras (fase siguiente) recalculará esto mismo al ingresar mercadería.
        private const decimal MargenPorDefecto = 1.25m;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximoImagen = 2 * 1024 * 1024; // 2 MB

        private readonly PuntoVentaContext _db;
        private readonly IWebHostEnvironment _entorno;

        public ProductosController(PuntoVentaContext db, IWebHostEnvironment entorno)
        {
            _db = db;
            _entorno = entorno;
        }

        // GET: api/Productos?buscar=texto
        // "buscar" encuentra por nombre (contiene) o por código exacto si es un número (id_producto)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos([FromQuery] string? buscar)
        {
            var consulta = _db.Productos.Include(p => p.Marca).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                if (int.TryParse(texto, out var codigo))
                {
                    consulta = consulta.Where(p => p.IdProducto == codigo || p.NombreProducto.Contains(texto));
                }
                else
                {
                    consulta = consulta.Where(p => p.NombreProducto.Contains(texto));
                }
            }

            var lista = await consulta.OrderBy(p => p.NombreProducto).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Productos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            var producto = await _db.Productos.Include(p => p.Marca).AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto is null)
            {
                return NotFound(new { mensaje = "El producto no existe." });
            }
            return Mapear(producto);
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<ProductoDto>> PostProducto(ProductoDto dto)
        {
            if (!await _db.Marcas.AnyAsync(m => m.IdMarca == dto.IdMarca))
            {
                return BadRequest(new { mensaje = "La marca indicada no existe." });
            }

            var nombre = dto.Producto.Trim();
            if (await _db.Productos.AnyAsync(p => p.NombreProducto == nombre && p.IdMarca == dto.IdMarca))
            {
                return Conflict(new { mensaje = "Ya existe un producto con ese nombre para esa marca." });
            }

            var producto = new Producto { FechaIngreso = DateTime.Now, Existencia = 0 };
            Copiar(dto, producto);

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();

            await _db.Entry(producto).Reference(p => p.Marca).LoadAsync();
            return CreatedAtAction(nameof(GetProducto), new { id = producto.IdProducto }, Mapear(producto));
        }

        // PUT: api/Productos/5  (no modifica existencia: eso lo hace Compras/Ventas)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProducto(int id, ProductoDto dto)
        {
            if (id != dto.IdProducto)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return NotFound(new { mensaje = "El producto no existe." });
            }

            if (!await _db.Marcas.AnyAsync(m => m.IdMarca == dto.IdMarca))
            {
                return BadRequest(new { mensaje = "La marca indicada no existe." });
            }

            var nombre = dto.Producto.Trim();
            if (await _db.Productos.AnyAsync(p => p.NombreProducto == nombre && p.IdMarca == dto.IdMarca && p.IdProducto != id))
            {
                return Conflict(new { mensaje = "Ya existe otro producto con ese nombre para esa marca." });
            }

            Copiar(dto, producto);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return NotFound(new { mensaje = "El producto no existe." });
            }

            try
            {
                _db.Productos.Remove(producto);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el producto tiene compras o ventas registradas." });
            }

            BorrarArchivoImagen(producto.Imagen);

            return NoContent();
        }

        // POST: api/Productos/5/imagen  (multipart/form-data, campo "archivo")
        [HttpPost("{id:int}/imagen")]
        [RequestSizeLimit(TamanoMaximoImagen)]
        public async Task<ActionResult<ProductoDto>> SubirImagen(int id, IFormFile archivo)
        {
            var producto = await _db.Productos.Include(p => p.Marca).FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return NotFound(new { mensaje = "El producto no existe." });
            }

            if (archivo is null || archivo.Length == 0)
            {
                return BadRequest(new { mensaje = "No se recibió ninguna imagen." });
            }

            if (archivo.Length > TamanoMaximoImagen)
            {
                return BadRequest(new { mensaje = "La imagen no puede superar los 2 MB." });
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return BadRequest(new { mensaje = "Solo se permiten imágenes JPG, PNG o WEBP." });
            }

            BorrarArchivoImagen(producto.Imagen);

            var carpeta = Path.Combine(_entorno.WebRootPath, "images", "productos");
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"prod_{id}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var destino = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(destino);
            }

            producto.Imagen = $"/images/productos/{nombreArchivo}";
            await _db.SaveChangesAsync();

            return Mapear(producto);
        }

        // DELETE: api/Productos/5/imagen
        [HttpDelete("{id:int}/imagen")]
        public async Task<IActionResult> EliminarImagen(int id)
        {
            var producto = await _db.Productos.FirstOrDefaultAsync(p => p.IdProducto == id);
            if (producto is null)
            {
                return NotFound(new { mensaje = "El producto no existe." });
            }

            BorrarArchivoImagen(producto.Imagen);
            producto.Imagen = null;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private void BorrarArchivoImagen(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa))
            {
                return;
            }

            try
            {
                var rutaFisica = Path.Combine(_entorno.WebRootPath, rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(rutaFisica))
                {
                    System.IO.File.Delete(rutaFisica);
                }
            }
            catch (IOException)
            {
                // si el archivo está en uso o ya no existe, no se interrumpe la operación principal
            }
        }

        private static void Copiar(ProductoDto dto, Producto producto)
        {
            producto.NombreProducto = dto.Producto.Trim();
            producto.IdMarca = dto.IdMarca;
            producto.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
            producto.PrecioCosto = dto.PrecioCosto;
            producto.PrecioVenta = dto.PrecioVenta > 0 ? dto.PrecioVenta : Math.Round(dto.PrecioCosto * MargenPorDefecto, 2);
        }

        private static ProductoDto Mapear(Producto p) => new ProductoDto
        {
            IdProducto = p.IdProducto,
            Producto = p.NombreProducto,
            IdMarca = p.IdMarca,
            Marca = p.Marca?.NombreMarca,
            Descripcion = p.Descripcion,
            Imagen = p.Imagen,
            PrecioCosto = p.PrecioCosto,
            PrecioVenta = p.PrecioVenta,
            Existencia = p.Existencia,
            FechaIngreso = p.FechaIngreso
        };
    }
}
