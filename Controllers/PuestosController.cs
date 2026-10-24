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
    public class PuestosController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public PuestosController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Puestos?buscar=texto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PuestoDto>>> GetPuestos([FromQuery] string? buscar)
        {
            var consulta = _db.Puestos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                consulta = consulta.Where(p => p.NombrePuesto.Contains(texto));
            }

            var lista = await consulta.OrderBy(p => p.NombrePuesto).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Puestos/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PuestoDto>> GetPuesto(int id)
        {
            var puesto = await _db.Puestos.AsNoTracking().FirstOrDefaultAsync(p => p.IdPuesto == id);
            if (puesto is null)
            {
                return NotFound(new { mensaje = "El puesto no existe." });
            }
            return Mapear(puesto);
        }

        // POST: api/Puestos
        [HttpPost]
        public async Task<ActionResult<PuestoDto>> PostPuesto(PuestoDto dto)
        {
            var nombre = dto.Puesto.Trim();

            if (await _db.Puestos.AnyAsync(p => p.NombrePuesto == nombre))
            {
                return Conflict(new { mensaje = "Ya existe un puesto con ese nombre." });
            }

            var puesto = new Puesto { NombrePuesto = nombre };
            _db.Puestos.Add(puesto);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPuesto), new { id = puesto.IdPuesto }, Mapear(puesto));
        }

        // PUT: api/Puestos/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutPuesto(int id, PuestoDto dto)
        {
            if (id != dto.IdPuesto)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var puesto = await _db.Puestos.FirstOrDefaultAsync(p => p.IdPuesto == id);
            if (puesto is null)
            {
                return NotFound(new { mensaje = "El puesto no existe." });
            }

            var nombre = dto.Puesto.Trim();
            if (await _db.Puestos.AnyAsync(p => p.NombrePuesto == nombre && p.IdPuesto != id))
            {
                return Conflict(new { mensaje = "Ya existe un puesto con ese nombre." });
            }

            puesto.NombrePuesto = nombre;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Puestos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePuesto(int id)
        {
            var puesto = await _db.Puestos.FirstOrDefaultAsync(p => p.IdPuesto == id);
            if (puesto is null)
            {
                return NotFound(new { mensaje = "El puesto no existe." });
            }

            try
            {
                _db.Puestos.Remove(puesto);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: hay empleados con este puesto." });
            }

            return NoContent();
        }

        private static PuestoDto Mapear(Puesto p) => new PuestoDto
        {
            IdPuesto = p.IdPuesto,
            Puesto = p.NombrePuesto
        };
    }
}
