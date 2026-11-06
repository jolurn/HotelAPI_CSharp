using HotelAPI.Data;
using HotelAPI.DTOs;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HabitacionesController : ControllerBase
    {
        private readonly HotelContext _context;

        public HabitacionesController(HotelContext context) => _context = context;

        // GET: api/Habitaciones
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var habitaciones = await _context.Habitaciones
                .AsNoTracking()
                .Select(h => new HabitacionLecturaDTO
                {
                    Id = h.Id,
                    Numero = h.Numero,
                    Precio = h.Precio,
                    NombreHotel = h.Hotel != null ? h.Hotel.Nombre : "Sin Hotel",
                    DireccionHotel = h.Hotel != null ? h.Hotel.Direccion : "Sin Dirección"
                })
                .ToListAsync();

            return Ok(habitaciones);
        }

        // GET: api/Habitaciones/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var habitacion = await _context.Habitaciones
                .AsNoTracking()
                .Where(h => h.Id == id)
                .Select(h => new HabitacionLecturaDTO
                {
                    Id = h.Id,
                    Numero = h.Numero,
                    Precio = h.Precio,
                    NombreHotel = h.Hotel != null ? h.Hotel.Nombre : "Sin Hotel",
                    DireccionHotel = h.Hotel != null ? h.Hotel.Direccion : "Sin Dirección"
                })
                .FirstOrDefaultAsync();

            if (habitacion == null)
                return NotFound(new { mensaje = "Habitación no encontrada" });

            return Ok(habitacion);
        }

        // POST: api/Habitaciones
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Habitacion habitacion)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Habitaciones.Add(habitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = habitacion.Id }, habitacion);
        }

        // PUT: api/Habitaciones/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Habitacion habitacion)
        {
            if (id != habitacion.Id)
                return BadRequest(new { mensaje = "El ID no coincide" });

            _context.Entry(habitacion).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Habitaciones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
                return NotFound(new { mensaje = "Habitación no encontrada" });

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
