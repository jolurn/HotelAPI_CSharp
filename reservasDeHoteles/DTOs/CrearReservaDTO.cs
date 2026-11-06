using System.ComponentModel.DataAnnotations;

namespace HotelAPI.DTOs
{
    // ==========================================
    // DTO DE ENTRADA (Lo que el cliente NOS ENVÍA)
    // ==========================================
    // Este DTO NO es igual a la entidad 'Reserva'. Es una versión simplificada
    // para que el cliente envíe SOLO lo necesario y no pueda hackear nuestra base de datos.
    public class CrearReservaDTO
    {
        // [Required]: Data Annotation. Si el cliente no envía este campo, 
        // .NET devuelve automáticamente un error HTTP 400 (Bad Request) ANTES de llegar al controlador.
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }

        // [MinLength(1)]: Valida que la lista no esté vacía. 
        // Un usuario no puede hacer una reserva si no selecciona al menos 1 habitación.
        [Required(ErrorMessage = "Debe incluir al menos una habitación")]
        [MinLength(1, ErrorMessage = "La reserva debe tener al menos 1 habitación")]
        public List<DetalleDTO> Detalles { get; set; } = new();

        // 👇 AQUÍ ESTÁ LA MAGIA DEL MUCHOS A MUCHOS 👇
        // En lugar de recibir los objetos de servicio completos, 
        // el cliente solo envía una lista de IDs (ej. [1, 2]). 
        // El repositorio se encargará de buscar esos servicios en la BD y relacionarlos.
        public List<int> ServiciosAdicionalesIds { get; set; } = new();
    }
}
