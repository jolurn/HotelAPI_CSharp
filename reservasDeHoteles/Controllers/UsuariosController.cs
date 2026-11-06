using HotelAPI.Data;
using HotelAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers
{
    // [ApiController]: Le dice a .NET que esta clase maneja peticiones HTTP (Swagger, Postman).
    // [Route("api/[controller]")]: La URL será /api/Usuarios (toma el nombre de la clase sin 'Controller').
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly HotelContext _context;

        // ==========================================
        // PASO 2: INYECCIÓN DE DEPENDENCIAS
        // ==========================================
        // Cuando llega una petición, .NET crea el HotelContext (conexión a BD) 
        // y lo inyecta aquí automáticamente. No hacemos 'new HotelContext()'.
        public UsuariosController(HotelContext context) => _context = context;

        // ==========================================
        // PASO 3: GET (LEER TODOS LOS USUARIOS)
        // ==========================================
        // Ruta: GET /api/Usuarios
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // 1. _context.Usuarios: Apunta a la tabla Usuarios.
            // 2. AsNoTracking(): OPTIMIZACIÓN. Le dice a EF Core: "No guardes estos datos 
            //    en memoria para rastrear cambios, solo los voy a leer". Es un 40% más rápido.
            // 3. ToListAsync(): Ejecuta el SQL (SELECT * FROM Usuarios) y lo pasa a una lista.
            var usuarios = await _context.Usuarios.AsNoTracking().ToListAsync();

            // 4. Ok(): Devuelve un código HTTP 200 (Éxito) con la lista en formato JSON.
            return Ok(usuarios);
        }

        // ==========================================
        // PASO 4: GET POR ID (LEER UN USUARIO ESPECÍFICO)
        // ==========================================
        // Ruta: GET /api/Usuarios/1
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Busca el primer usuario cuyo Id coincida. (SELECT TOP 1 * FROM Usuarios WHERE Id = 1)
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            // Si el usuario no existe en la BD, devolvemos un HTTP 404 (Not Found).
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            // Si existe, devolvemos un HTTP 200 con sus datos.
            return Ok(usuario);
        }

        // ==========================================
        // PASO 5: POST (CREAR UN NUEVO USUARIO)
        // ==========================================
        // Ruta: POST /api/Usuarios
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Usuario usuario)
        {
            // [FromBody]: Toma el JSON que envía el cliente y lo convierte en el objeto 'usuario'.

            // Valida si hay errores (por ahora no tenemos Data Annotations en el modelo, pero es buena práctica).
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Add(): Prepara el INSERT en la memoria de EF Core (aún no toca SQL Server).
            _context.Usuarios.Add(usuario);

            // 2. SaveChangesAsync(): Aquí SE EJECUTA EL INSERT real en SQL Server. 
            // Además, SQL Server genera el 'Id' automáticamente y EF Core lo actualiza en el objet
            await _context.SaveChangesAsync();

            // 3. CreatedAtAction(): Devuelve HTTP 201 (Created). Le dice al cliente la URL 
            // donde puede consultar el usuario recién creado.
            return CreatedAtAction(nameof(Get), new { id = usuario.Id }, usuario);
        }

        // ==========================================
        // PASO 6: PUT (ACTUALIZAR UN USUARIO)
        // ==========================================
        // Ruta: PUT /api/Usuarios/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Usuario usuario)
        {
            // Seguridad: El ID de la URL debe ser el mismo que el ID del JSON.
            if (id != usuario.Id) return BadRequest();

            // Entry().State = Modified: Le dice a EF Core "Este objeto cambió, prepárate para hacer un UPDATE".
            _context.Entry(usuario).State = EntityState.Modified;

            // Ejecuta el UPDATE en SQL Server.
            await _context.SaveChangesAsync();

            // HTTP 204 (No Content): Significa "Actualizado con éxito, pero no tengo datos que devolverte".
            return NoContent();
        }

        // ==========================================
        // PASO 7: DELETE (ELIMINAR UN USUARIO)
        // ==========================================
        // Ruta: DELETE /api/Usuarios/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // FindAsync: Busca por llave primaria (es súper rápido porque va directo al índice de SQL).
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // Remove(): Prepara el DELETE en memoria.
            _context.Usuarios.Remove(usuario);

            // Ejecuta el DELETE real en SQL Server.
            await _context.SaveChangesAsync();

            // HTTP 204 (No Content): Eliminado con éxito.
            return NoContent();
        }
    }
}
