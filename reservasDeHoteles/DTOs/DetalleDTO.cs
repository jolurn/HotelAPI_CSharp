using System.ComponentModel.DataAnnotations;

namespace HotelAPI.DTOs
{
    // ==========================================
    // DTO DE DETALLE (El "Ítem" del carrito)
    // ==========================================
    // Este DTO reemplaza a la entidad 'ReservaDetalle' en la entrada de datos.
    // ¿Notas que no tiene el campo 'Subtotal'? ¡Exacto! 
    // El cliente no debe enviar el precio, porque podría enviar 'Subtotal: 0' y 
    // quedarse con la habitación gratis. El servidor lo calculará.
    public class DetalleDTO
    {
        [Required]
        public int HabitacionId { get; set; }

        // [Range]: Data Annotation que limita el número de noches.
        // Si intentan reservar 0 noches o 100 noches, la API los rechazará automáticamente.
        [Range(1, 30, ErrorMessage = "Las noches deben ser entre 1 y 30")]
        public int Noches { get; set; }
    }
}
