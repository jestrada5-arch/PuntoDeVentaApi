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
    public class EmpleadosController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public EmpleadosController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Empleados?buscar=texto  (busca por nombres, apellidos o DPI)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetEmpleados([FromQuery] string? buscar)
        {
            var consulta = _db.Empleados.Include(e => e.Puesto).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                consulta = consulta.Where(e =>
                    e.Nombres.Contains(texto) ||
                    e.Apellidos.Contains(texto) ||
                    e.Dpi.Contains(texto));
            }

            var lista = await consulta.OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Empleados/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmpleadoDto>> GetEmpleado(int id)
        {
            var empleado = await _db.Empleados.Include(e => e.Puesto).AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdEmpleado == id);

            if (empleado is null)
            {
                return NotFound(new { mensaje = "El empleado no existe." });
            }
            return Mapear(empleado);
        }

        // POST: api/Empleados
        [HttpPost]
        public async Task<ActionResult<EmpleadoDto>> PostEmpleado(EmpleadoDto dto)
        {
            var dpi = dto.Dpi.Trim();

            if (await _db.Empleados.AnyAsync(e => e.Dpi == dpi))
            {
                return Conflict(new { mensaje = "Ya existe un empleado con ese DPI." });
            }

            if (!await _db.Puestos.AnyAsync(p => p.IdPuesto == dto.IdPuesto))
            {
                return BadRequest(new { mensaje = "El puesto indicado no existe." });
            }

            var empleado = new Empleado { FechaIngreso = DateTime.Now };
            Copiar(dto, empleado);

            _db.Empleados.Add(empleado);
            await _db.SaveChangesAsync();

            await _db.Entry(empleado).Reference(e => e.Puesto).LoadAsync();
            return CreatedAtAction(nameof(GetEmpleado), new { id = empleado.IdEmpleado }, Mapear(empleado));
        }

        // PUT: api/Empleados/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutEmpleado(int id, EmpleadoDto dto)
        {
            if (id != dto.IdEmpleado)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == id);
            if (empleado is null)
            {
                return NotFound(new { mensaje = "El empleado no existe." });
            }

            var dpi = dto.Dpi.Trim();
            if (await _db.Empleados.AnyAsync(e => e.Dpi == dpi && e.IdEmpleado != id))
            {
                return Conflict(new { mensaje = "Ya existe otro empleado con ese DPI." });
            }

            if (!await _db.Puestos.AnyAsync(p => p.IdPuesto == dto.IdPuesto))
            {
                return BadRequest(new { mensaje = "El puesto indicado no existe." });
            }

            Copiar(dto, empleado);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Empleados/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.IdEmpleado == id);
            if (empleado is null)
            {
                return NotFound(new { mensaje = "El empleado no existe." });
            }

            try
            {
                _db.Empleados.Remove(empleado);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el empleado tiene ventas o un usuario asociado." });
            }

            return NoContent();
        }

        private static void Copiar(EmpleadoDto dto, Empleado empleado)
        {
            empleado.Nombres = dto.Nombres.Trim();
            empleado.Apellidos = dto.Apellidos.Trim();
            empleado.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
            empleado.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            empleado.Dpi = dto.Dpi.Trim();
            empleado.Genero = dto.Genero;
            empleado.FechaNacimiento = dto.FechaNacimiento;
            empleado.IdPuesto = dto.IdPuesto;
            empleado.FechaInicioLabores = dto.FechaInicioLabores;
        }

        private static EmpleadoDto Mapear(Empleado e) => new EmpleadoDto
        {
            IdEmpleado = e.IdEmpleado,
            Nombres = e.Nombres,
            Apellidos = e.Apellidos,
            Direccion = e.Direccion,
            Telefono = e.Telefono,
            Dpi = e.Dpi,
            Genero = e.Genero,
            FechaNacimiento = e.FechaNacimiento,
            IdPuesto = e.IdPuesto,
            Puesto = e.Puesto?.NombrePuesto,
            FechaInicioLabores = e.FechaInicioLabores,
            FechaIngreso = e.FechaIngreso
        };
    }
}
