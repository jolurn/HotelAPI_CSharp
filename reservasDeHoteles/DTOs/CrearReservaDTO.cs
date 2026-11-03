using System.ComponentModel.DataAnnotations;

namespace HotelAPI.DTOs
{
    public class CrearReservaDTO
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio")]
        public int UsuarioId { get; set; }
               
        [Required(ErrorMessage = "Debe incluir al menos una habitación")]
        [MinLength(1, ErrorMessage = "La reserva debe tener al menos 1 habitación")]
        public List<DetalleDTO> Detalles { get; set; } = new();
                
        public List<int> ServiciosAdicionalesIds { get; set; } = new();
    }
}
