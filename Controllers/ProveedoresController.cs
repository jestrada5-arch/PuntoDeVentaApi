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
    public class ProveedoresController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public ProveedoresController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Proveedores?buscar=texto  (busca por nombre o NIT)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> GetProveedores([FromQuery] string? buscar)
        {
            var consulta = _db.Proveedores.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                consulta = consulta.Where(p => p.NombreProveedor.Contains(texto) || p.Nit.Contains(texto));
            }

            var lista = await consulta.OrderBy(p => p.NombreProveedor).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Proveedores/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProveedorDto>> GetProveedor(int id)
        {
            var proveedor = await _db.Proveedores.AsNoTracking().FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return NotFound(new { mensaje = "El proveedor no existe." });
            }
            return Mapear(proveedor);
        }

        // POST: api/Proveedores
        [HttpPost]
        public async Task<ActionResult<ProveedorDto>> PostProveedor(ProveedorDto dto)
        {
            var nit = dto.Nit.Trim().ToUpperInvariant();

            if (await _db.Proveedores.AnyAsync(p => p.Nit == nit))
            {
                return Conflict(new { mensaje = "Ya existe un proveedor con ese NIT." });
            }

            var proveedor = new Proveedor();
            Copiar(dto, proveedor);

            _db.Proveedores.Add(proveedor);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProveedor), new { id = proveedor.IdProveedor }, Mapear(proveedor));
        }

        // PUT: api/Proveedores/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutProveedor(int id, ProveedorDto dto)
        {
            if (id != dto.IdProveedor)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return NotFound(new { mensaje = "El proveedor no existe." });
            }

            var nit = dto.Nit.Trim().ToUpperInvariant();
            if (await _db.Proveedores.AnyAsync(p => p.Nit == nit && p.IdProveedor != id))
            {
                return Conflict(new { mensaje = "Ya existe otro proveedor con ese NIT." });
            }

            Copiar(dto, proveedor);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Proveedores/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _db.Proveedores.FirstOrDefaultAsync(p => p.IdProveedor == id);
            if (proveedor is null)
            {
                return NotFound(new { mensaje = "El proveedor no existe." });
            }

            try
            {
                _db.Proveedores.Remove(proveedor);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el proveedor tiene compras registradas." });
            }

            return NoContent();
        }

        private static void Copiar(ProveedorDto dto, Proveedor proveedor)
        {
            proveedor.NombreProveedor = dto.Proveedor.Trim();
            proveedor.Nit = dto.Nit.Trim().ToUpperInvariant();
            proveedor.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
            proveedor.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        }

        private static ProveedorDto Mapear(Proveedor p) => new ProveedorDto
        {
            IdProveedor = p.IdProveedor,
            Proveedor = p.NombreProveedor,
            Nit = p.Nit,
            Direccion = p.Direccion,
            Telefono = p.Telefono
        };
    }
}
