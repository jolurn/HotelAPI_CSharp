namespace HotelAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        
        public List<Reserva> Reservas { get; set; } = new();
    }
}
