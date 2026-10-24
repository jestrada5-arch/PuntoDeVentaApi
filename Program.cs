using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVentaApi.Data;
using PuntoVentaApi.Models;
using PuntoVentaApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// ---------- Base de datos MySQL ----------
var connectionString = builder.Configuration.GetConnectionString("ConexionMySQL")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'ConexionMySQL'. Configúrela con dotnet user-secrets.");

builder.Services.AddDbContext<PuntoVentaContext>(opt => opt.UseMySQL(connectionString));

// ---------- JWT ----------
var jwt = builder.Configuration.GetSection("Jwt");
var clave = jwt["Key"];
if (string.IsNullOrWhiteSpace(clave) || clave.Length < 32)
{
    throw new InvalidOperationException("Falta 'Jwt:Key' (mínimo 32 caracteres). Configúrela con dotnet user-secrets.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// ---------- Usuario administrador inicial (solo si no hay usuarios) ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<PuntoVentaContext>();
        if (!db.Usuarios.Any())
        {
            var admin = app.Configuration.GetSection("AdminInicial");
            var usuario = admin["Usuario"];
            var password = admin["Password"];

            if (!string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(password))
            {
                db.Usuarios.Add(new Usuario
                {
                    NombreUsuario = usuario,
                    Password = BCrypt.Net.BCrypt.HashPassword(password)
                });
                db.SaveChanges();
                app.Logger.LogInformation("Se creó el usuario inicial '{Usuario}'.", usuario);
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudo verificar o crear el usuario inicial. Revise la conexión a MySQL.");
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
