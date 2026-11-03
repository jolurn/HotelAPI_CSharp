namespace HotelAPI.DTOs
{
    public class ReservaCompletaDTO
    {
        public int IdReserva { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public decimal Total { get; set; }

        
        public List<string> HabitacionesReservadas { get; set; } = new();

        
        public List<string> ServiciosAdicionales { get; set; } = new();
    }
}
