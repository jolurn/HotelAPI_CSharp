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
    public class ReservaDetallesController : ControllerBase
    {
        private readonly HotelContext _context;

        public ReservaDetallesController(HotelContext context) => _context = context;

        // GET: api/ReservaDetalles
        [HttpGet]
        public async Task<IActionResult> Get()
        {            
            var detalles = await _context.ReservaDetalles
                .AsNoTracking()
                .Select(rd => new ReservaDetalleLecturaDTO
                {
                    Id = rd.Id,
                    Noches = rd.Noches,
                    Subtotal = rd.Subtotal,
                    NumeroHabitacion = rd.Habitacion != null ? rd.Habitacion.Numero : "N/A",
                    PrecioHabitacion = rd.Habitacion != null ? rd.Habitacion.Precio : 0,
                    FechaReserva = rd.Reserva != null ? rd.Reserva.FechaReserva : DateTime.MinValue
                })
                .ToListAsync();

            return Ok(detalles);
        }

        // GET: api/ReservaDetalles/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Usamos proyección (Select) para evitar el ciclo de referencias y devolver un DTO limpio
            var detalle = await _context.ReservaDetalles
                .AsNoTracking()
                .Where(rd => rd.Id == id)
                .Select(rd => new ReservaDetalleLecturaDTO
                {
                    Id = rd.Id,
                    Noches = rd.Noches,
                    Subtotal = rd.Subtotal,
                    NumeroHabitacion = rd.Habitacion != null ? rd.Habitacion.Numero : "N/A",
                    PrecioHabitacion = rd.Habitacion != null ? rd.Habitacion.Precio : 0,
                    FechaReserva = rd.Reserva != null ? rd.Reserva.FechaReserva : DateTime.MinValue
                })
                .FirstOrDefaultAsync();

            if (detalle == null)
                return NotFound(new { mensaje = "Detalle no encontrado" });

            return Ok(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ReservaDetalle detalle)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
                        
            var habitacion = await _context.Habitaciones.FindAsync(detalle.HabitacionId);
            if (habitacion == null) return BadRequest(new { mensaje = "Habitación no existe" });

            detalle.Subtotal = habitacion.Precio * detalle.Noches;

            _context.ReservaDetalles.Add(detalle);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = detalle.Id }, detalle);
        }
                
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ReservaDetalle detalle)
        {
            if (id != detalle.Id) return BadRequest();
                        
            var habitacion = await _context.Habitaciones.FindAsync(detalle.HabitacionId);
            if (habitacion != null)
            {
                detalle.Subtotal = habitacion.Precio * detalle.Noches;
            }

            _context.Entry(detalle).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var detalle = await _context.ReservaDetalles.FindAsync(id);
            if (detalle == null) return NotFound();
            _context.ReservaDetalles.Remove(detalle);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
