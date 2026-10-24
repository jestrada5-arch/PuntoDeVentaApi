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
    public class ClientesController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public ClientesController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Clientes?buscar=texto  (busca por nombres, apellidos o NIT)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes([FromQuery] string? buscar)
        {
            var consulta = _db.Clientes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                consulta = consulta.Where(c =>
                    c.Nombres.Contains(texto) ||
                    c.Apellidos.Contains(texto) ||
                    c.Nit.Contains(texto));
            }

            var lista = await consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync();
            return lista.Select(Mapear).ToList();
        }

        // GET: api/Clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _db.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
            if (cliente is null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
            }
            return Mapear(cliente);
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente(ClienteDto dto)
        {
            var nit = dto.Nit.Trim();

            if (await _db.Clientes.AnyAsync(c => c.Nit == nit))
            {
                return Conflict(new { mensaje = "Ya existe un cliente con ese NIT." });
            }

            var cliente = new Cliente { FechaIngreso = DateTime.Now };
            Copiar(dto, cliente);

            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, Mapear(cliente));
        }

        // PUT: api/Clientes/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCliente(int id, ClienteDto dto)
        {
            if (id != dto.IdCliente)
            {
                return BadRequest(new { mensaje = "El id de la URL no coincide con el del cuerpo." });
            }

            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);
            if (cliente is null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
            }

            var nit = dto.Nit.Trim();
            if (await _db.Clientes.AnyAsync(c => c.Nit == nit && c.IdCliente != id))
            {
                return Conflict(new { mensaje = "Ya existe otro cliente con ese NIT." });
            }

            Copiar(dto, cliente);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);
            if (cliente is null)
            {
                return NotFound(new { mensaje = "El cliente no existe." });
            }

            try
            {
                _db.Clientes.Remove(cliente);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se puede eliminar: el cliente tiene ventas registradas." });
            }

            return NoContent();
        }

        private static void Copiar(ClienteDto dto, Cliente cliente)
        {
            cliente.Nombres = dto.Nombres.Trim();
            cliente.Apellidos = dto.Apellidos.Trim();
            cliente.Nit = dto.Nit.Trim().ToUpperInvariant();
            cliente.Genero = dto.Genero;
            cliente.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
            cliente.CorreoElectronico = string.IsNullOrWhiteSpace(dto.CorreoElectronico) ? null : dto.CorreoElectronico.Trim();
        }

        private static ClienteDto Mapear(Cliente c) => new ClienteDto
        {
            IdCliente = c.IdCliente,
            Nombres = c.Nombres,
            Apellidos = c.Apellidos,
            Nit = c.Nit,
            Genero = c.Genero,
            Telefono = c.Telefono,
            CorreoElectronico = c.CorreoElectronico,
            FechaIngreso = c.FechaIngreso
        };
    }
}
