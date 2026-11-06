using HotelAPI.Data;
using HotelAPI.DTOs;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization; // 👈 Necesario para proteger endpoints
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HotelContext _context;
        private readonly IConfiguration _config;

        public AuthController(HotelContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ==========================================
        // 1. REGISTRO (POST: api/auth/registro)
        // ==========================================
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroUsuarioDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Verificamos si el correo ya existe en la base de datos
            var existe = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (existe) return BadRequest(new { mensaje = "El correo ya está registrado." });

            // 👇 MAGIA DE BCRYPT: Encriptamos la contraseña 👇
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = passwordHash,
                EstaBloqueado = false // 👈 Nace sin bloqueos
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado exitosamente." });
        }

        // ==========================================
        // 2. LOGIN (POST: api/auth/login)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Buscamos al usuario por su Email
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            // 👇 VERIFICAMOS SI ESTÁ EN LA LISTA NEGRA (Usando la función privada) 👇
            if (VerificarSiEstaBloqueado(usuario))
            {
                return StatusCode(403, new { mensaje = "Acceso denegado. Tu cuenta está bloqueada por un administrador." });
            }

            // 👇 MAGIA DE BCRYPT: Comparamos la contraseña ingresada con el Hash 👇
            bool esValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
            if (!esValido)
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            // --- GENERACIÓN DEL JWT ---

            // 1. Creamos los Claims (Datos dentro del token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, "Administrador") // 👈 ROL PARA PROTEGER ENDPOINTS
            };

            // 2. Leemos la llave secreta
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3. Creamos el Token
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_config["JwtSettings:MinutesExpiration"]!)),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                mensaje = "Login exitoso",
                token = tokenString,
                email = usuario.Email,
                nombre = usuario.Nombre
            });
        }

        // ==========================================
        // 3. BLOQUEAR/DESBLOQUEAR (PATCH: api/auth/{id}/bloquear)
        // ==========================================
        // 👇 SOLO UN ADMINISTRADOR CON TOKEN VÁLIDO PUEDE ENTRAR AQUÍ 👇
        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id}/bloquear")]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool bloquear)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            // 👇 Usamos la función privada para aplicar el cambio 👇
            AplicarEstadoBloqueo(usuario, bloquear);

            await _context.SaveChangesAsync();

            string estado = bloquear ? "bloqueado" : "desbloqueado";
            return Ok(new { mensaje = $"El usuario ha sido {estado} correctamente." });
        }

        // ==========================================
        // 👇 FUNCIONES PRIVADAS (Lógica encapsulada) 👇
        // ==========================================

        // Verifica si el usuario está en la lista negra (bloqueado)
        private bool VerificarSiEstaBloqueado(Usuario usuario)
        {
            return usuario.EstaBloqueado;
        }

        // Aplica el cambio de estado de bloqueo
        private void AplicarEstadoBloqueo(Usuario usuario, bool estado)
        {
            usuario.EstaBloqueado = estado;
        }
    }
}