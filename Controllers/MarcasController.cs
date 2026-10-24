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
    public class MarcasController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public MarcasController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Marcas?buscar=texto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarcaDto>>> GetMarcas([FromQuery] string? buscar)
        {
            var consulta = _db.Marcas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                consulta = consulta.Where(m => m.NombreMarca.Contains(texto));
            }

            var lista = await consulta.OrderBy(m => m.NombreMarca).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Marcas/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MarcaDto>> GetMarca(int id)
        {
            var marca = await _db.Marcas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMarca == id);
            if (marca is null)
            {
                return NotFound(new { mensaje = "La marca no existe." });
            }
            return Mapear(marca);
        }

        // POST: api/Marcas
        [HttpPost]
        public async Task<ActionResult<MarcaDto>> PostMarca(MarcaDto dto)
        {
            var nombre = dto.Marca.Trim();

            if (await _db.Marcas.AnyAsync(m => m.NombreMarca == nombre))
            {
                return Conflict(new { mensaje = "Ya existe una marca con ese nombre." });
            }

            var marca = new Marca { NombreMarca = nombre };
            _db.Marcas.Add(marca);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMarca), new { id = marca.IdMarca }, Mapear(marca));
        }

        // PUT: api/Marcas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMarca(int id, MarcaDto dto)
        {
            if (id != dto.IdMarca)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var marca = await _db.Marcas.FirstOrDefaultAsync(m => m.IdMarca == id);
            if (marca is null)
            {
                return NotFound(new { mensaje = "La marca no existe." });
            }

            var nombre = dto.Marca.Trim();
            if (await _db.Marcas.AnyAsync(m => m.NombreMarca == nombre && m.IdMarca != id))
            {
                return Conflict(new { mensaje = "Ya existe una marca con ese nombre." });
            }

            marca.NombreMarca = nombre;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Marcas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            var marca = await _db.Marcas.FirstOrDefaultAsync(m => m.IdMarca == id);
            if (marca is null)
            {
                return NotFound(new { mensaje = "La marca no existe." });
            }

            try
            {
                _db.Marcas.Remove(marca);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: hay productos con esta marca." });
            }

            return NoContent();
        }

        private static MarcaDto Mapear(Marca m) => new MarcaDto
        {
            IdMarca = m.IdMarca,
            Marca = m.NombreMarca
        };
    }
}
