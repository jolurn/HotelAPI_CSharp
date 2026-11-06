using HotelAPI.Data;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization; // 👈 Necesario para [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HotelesController : ControllerBase
    {
        private readonly HotelContext _context;

        public HotelesController(HotelContext context) => _context = context;

        // ==========================================
        // ENDPOINT PÚBLICO (No requiere Token)
        // ==========================================
        // Ruta: GET /api/Hoteles/catalogo
        // Cualquier usuario, incluso sin loguearse, puede ver los hoteles.
        [HttpGet("catalogo")]
        public async Task<IActionResult> GetCatalogoPublico()
        {
            var hoteles = await _context.Hoteles.AsNoTracking().ToListAsync();
            return Ok(new
            {
                mensaje = "Catálogo público. No necesitas token para ver esto.",
                data = hoteles
            });
        }

        // ==========================================
        // ENDPOINT PRIVADO (Requiere Token JWT)
        // ==========================================
        // Ruta: POST /api/Hoteles/registrar
        // Solo los usuarios con un token válido (Admin) pueden crear hoteles.
        [Authorize] // 👈 EL GUARDAESPALDAS ESTÁ AQUÍ
        [HttpPost("registrar")]
        public async Task<IActionResult> PostHotelPrivado([FromBody] Hotel hotel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Hoteles.Add(hotel);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Hotel registrado exitosamente. Has usado un token válido.",
                hotel
            });
        }

        // ==========================================
        // ENDPOINTS RESTANTES (GET por ID, PUT, DELETE)
        // ==========================================
        // Para mantener la consistencia, dejamos que estos también requieran token.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var hotel = await _context.Hoteles.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
            if (hotel == null) return NotFound();
            return Ok(hotel);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Hotel hotel)
        {
            if (id != hotel.Id) return BadRequest();
            _context.Entry(hotel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await _context.Hoteles.FindAsync(id);
            if (hotel == null) return NotFound();
            _context.Hoteles.Remove(hotel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}