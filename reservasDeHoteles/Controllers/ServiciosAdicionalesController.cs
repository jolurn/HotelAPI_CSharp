using HotelAPI.Data;
using HotelAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiciosAdicionalesController : ControllerBase
    {
        private readonly HotelContext _context;

        public ServiciosAdicionalesController(HotelContext context) => _context = context;
                
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.ServiciosAdicionales.AsNoTracking().ToListAsync());
        }
                
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var servicio = await _context.ServiciosAdicionales.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (servicio == null) return NotFound(new { mensaje = "Servicio no encontrado" });
            return Ok(servicio);
        }
                
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ServicioAdicional servicio)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.ServiciosAdicionales.Add(servicio);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = servicio.Id }, servicio);
        }
                
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ServicioAdicional servicio)
        {
            if (id != servicio.Id) return BadRequest();
            _context.Entry(servicio).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
                
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var servicio = await _context.ServiciosAdicionales.FindAsync(id);
            if (servicio == null) return NotFound();
            _context.ServiciosAdicionales.Remove(servicio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
