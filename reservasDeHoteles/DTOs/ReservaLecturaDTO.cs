namespace HotelAPI.DTOs
{
    public class ReservaLecturaDTO
    {
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; }
        public decimal Total { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int CantidadHabitaciones { get; set; }
    }
}
