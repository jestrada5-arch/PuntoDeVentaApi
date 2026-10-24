using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVentaApi.Data;
using PuntoVentaApi.Models;
using PuntoVentaApi.Services;

namespace PuntoVentaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PuntoVentaContext _db;
        private readonly TokenService _tokens;

        public AuthController(PuntoVentaContext db, TokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        // POST: api/Auth/login  (público)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == dto.Usuario.Trim());

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password))
            {
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
            }

            var (token, expira) = _tokens.Crear(usuario);

            return new LoginRespuestaDto
            {
                Token = token,
                Usuario = usuario.NombreUsuario,
                Expira = expira
            };
        }

        // POST: api/Auth/registrar  (solo con sesión iniciada)
        [HttpPost("registrar")]
        [Authorize]
        public async Task<IActionResult> Registrar(RegistrarUsuarioDto dto)
        {
            var nombre = dto.Usuario.Trim();

            if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombre))
            {
                return Conflict(new { mensaje = "Ya existe un usuario con ese nombre." });
            }

            if (dto.IdEmpleado.HasValue && !await _db.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado.Value))
            {
                return BadRequest(new { mensaje = "El empleado indicado no existe." });
            }

            _db.Usuarios.Add(new Usuario
            {
                NombreUsuario = nombre,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdEmpleado = dto.IdEmpleado
            });
            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new { mensaje = "Usuario creado correctamente." });
        }
    }
}
