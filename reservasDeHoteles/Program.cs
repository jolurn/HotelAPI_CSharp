using HotelAPI.Data;
using HotelAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// ==========================================
// EL PUNTO DE ENTRADA (PROGRAM.CS)
// ==========================================
// Este archivo es el "tablero de mandos". Aquí se encienden los motores, 
// se configuran las conexiones a la base de datos y se prepara el servidor 
// (Kestrel) para recibir peticiones HTTP.

// 1. CREAMOS EL "BUILDER" (El constructor de la app)
var builder = WebApplication.CreateBuilder(args);

// ==========================================
// FASE 1: INYECCIÓN DE DEPENDENCIAS (Servicios)
// ==========================================
// Aquí le decimos a .NET qué herramientas va a necesitar nuestra API.

// Habilita el uso de Controladores (los que tienen [ApiController]).
builder.Services.AddControllers();

// Habilita el explorador de endpoints (necesario para que Swagger lea las rutas)
builder.Services.AddEndpointsApiExplorer();

// Configura Swagger para que podamos probar la API desde el navegador.
builder.Services.AddSwaggerGen();

// 👇 REGISTRAMOS EL ORM (Entity Framework) 👇
builder.Services.AddDbContext<HotelContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 👇 REGISTRAMOS EL MICROORM (Nuestro Repositorio Híbrido) 👇
builder.Services.AddScoped<ReservaRepository>();

// ==========================================
// NUEVO: CONFIGURACIÓN DE SEGURIDAD JWT (Sesión 1 - Modulo 3)
// ==========================================
// Le decimos a la API cómo validar los tokens que le envíen los usuarios.
// Esto asegura que solo las personas con un "pase de abordar" válido puedan entrar.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Verifica que nadie falsificó la firma del token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
            ValidateIssuer = true,           // Verifica quién emitió el token
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,         // Verifica para quién es el token (Audiencia)
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true          // Rechaza tokens caducados
        };
    });

// 2. CONSTRUIMOS LA APP
var app = builder.Build();

// ==========================================
// FASE 1.5: MIGRACIONES AUTOMÁTICAS (Solo Desarrollo)
// ==========================================
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<HotelContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Ocurrió un error al aplicar las migraciones de la base de datos.");
        }
    }
}

// ==========================================
// FASE 2: EL PIPELINE DE MIDDLEWARES (El flujo de la petición)
// ==========================================

// 🛡️ EL MIDDLEWARE GLOBAL DE EXCEPCIONES (Sesión 9)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Mensaje = "Ocurrió un error inesperado en el servidor.",
            Hora = DateTime.UtcNow
        };
        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// NUEVO: EL ORDEN DE LOS MIDDLEWARES DE SEGURIDAD IMPORTA
// ==========================================
// 1. UseAuthentication: Lee el Token de la cabecera y verifica QUIÉN es el usuario.
app.UseAuthentication();

// 2. UseAuthorization: Verifica si el usuario (ya autenticado) tiene PERMISO para entrar a esa ruta.
app.UseAuthorization();

// 🚀 LA LÍNEA MÁS IMPORTANTE 🚀
app.MapControllers();

// 3. ¡ENCENDER EL MOTOR!
app.Run();