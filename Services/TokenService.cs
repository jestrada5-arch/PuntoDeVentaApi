using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PuntoVentaApi.Models;

namespace PuntoVentaApi.Services
{
    // Genera el JSON Web Token (JWT) cuando el usuario inicia sesión correctamente
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, DateTime Expira) Crear(Usuario usuario)
        {
            var jwt = _config.GetSection("Jwt");
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var expira = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiraMinutos"] ?? "60"));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario)
                }),
                Expires = expira,
                Issuer = jwt["Issuer"],
                Audience = jwt["Audience"],
                SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return (token, expira);
        }
    }
}
