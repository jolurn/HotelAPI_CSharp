namespace HotelAPI.DTOs
{
    public class ReservaDetalleLecturaDTO
    {
        public int Id { get; set; }
        public int Noches { get; set; }
        public decimal Subtotal { get; set; }

        
        public string NumeroHabitacion { get; set; } = string.Empty;
        public decimal PrecioHabitacion { get; set; }
        public DateTime FechaReserva { get; set; }
    }
}
