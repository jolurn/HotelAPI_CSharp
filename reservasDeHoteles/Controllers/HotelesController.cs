using HotelAPI.Data;
using HotelAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HotelesController : ControllerBase
    {
        private readonly HotelContext _context;

        public HotelesController(HotelContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Get()
        {            
            return Ok(await _context.Hoteles.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var hotel = await _context.Hoteles.AsNoTracking().FirstOrDefaultAsync(h => h.Id == id);
            if (hotel == null) return NotFound();
            return Ok(hotel);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Hotel hotel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _context.Hoteles.Add(hotel);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = hotel.Id }, hotel);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Hotel hotel)
        {
            if (id != hotel.Id) return BadRequest();
            _context.Entry(hotel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

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
