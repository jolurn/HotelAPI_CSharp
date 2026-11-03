namespace HotelAPI.DTOs
{
    public class HabitacionLecturaDTO
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string NombreHotel { get; set; } = string.Empty;
        public string DireccionHotel { get; set; } = string.Empty;
    }
}
