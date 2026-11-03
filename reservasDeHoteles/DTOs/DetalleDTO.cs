using System.ComponentModel.DataAnnotations;

namespace HotelAPI.DTOs
{
    public class DetalleDTO
    {
        [Required]
        public int HabitacionId { get; set; }

        [Range(1, 30, ErrorMessage = "Las noches deben ser entre 1 y 30")]
        public int Noches { get; set; }
    }
}
