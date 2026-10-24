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
    public class MenuController : ControllerBase
    {
        private readonly PuntoVentaContext _db;

        public MenuController(PuntoVentaContext db)
        {
            _db = db;
        }

        // GET: api/Menu  -> árbol padre-hijo para construir el menú en Blazor
        [HttpGet]
        public async Task<ActionResult<List<MenuDto>>> GetMenu()
        {
            var opciones = await _db.Menu.AsNoTracking().OrderBy(m => m.Orden).ToListAsync();
            return Construir(opciones, null);
        }

        private static List<MenuDto> Construir(List<MenuOpcion> todas, int? idPadre)
        {
            return todas
                .Where(m => m.IdPadre == idPadre)
                .OrderBy(m => m.Orden)
                .Select(m => new MenuDto
                {
                    IdMenu = m.IdMenu,
                    Nombre = m.Nombre,
                    Url = m.Url,
                    Icono = m.Icono,
                    Hijos = Construir(todas, m.IdMenu)
                })
                .ToList();
        }
    }
}
