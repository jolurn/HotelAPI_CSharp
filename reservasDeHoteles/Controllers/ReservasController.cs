using HotelAPI.DTOs;
using HotelAPI.Models;
using HotelAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 👈 ¡EL GUARDAESPALDAS ESTÁ AQUÍ! 
    public class ReservasController : ControllerBase
    {
        private readonly ReservaRepository _repository;
                
        public ReservasController(ReservaRepository repository)
        {
            _repository = repository;
        }
                
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var reservas = await _repository.ObtenerTodasAsync();
            return Ok(reservas);
        }

        // GET: api/Reservas/reporte-completo
        [HttpGet("reporte-completo")]
        public async Task<IActionResult> GetReporteCompleto()
        {
            var reporte = await _repository.ObtenerReporteCompletoAsync();
            return Ok(reporte);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CrearReservaDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 👇 EXTRAEMOS EL ID DEL USUARIO DESDE EL TOKEN JWT 👇
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized(new { mensaje = "Token inválido o no proporcionado." });

            // Asignamos el ID del token al DTO (ignoramos el que venga en el JSON)
            dto.UsuarioId = int.Parse(userIdString);

            try
            {
                int id = await _repository.CrearReservaAsync(dto);
                return CreatedAtAction(nameof(Get), new { id }, new { mensaje = "Reserva creada con éxito", id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Reserva reserva)
        {
            if (id != reserva.Id)
                return BadRequest(new { mensaje = "El ID de la URL no coincide con el ID del JSON." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool actualizado = await _repository.ActualizarAsync(reserva);

            if (!actualizado)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            return NoContent(); 
        }
                
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool eliminado = await _repository.EliminarAsync(id);

            if (!eliminado)
                return NotFound(new { mensaje = "Reserva no encontrada." });

            return NoContent();
        }
    }
}
