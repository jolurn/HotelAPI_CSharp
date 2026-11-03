namespace HotelAPI.Models
{
    public class ServicioAdicional
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
                
        public ICollection<Reserva> Reservas { get; set; } = new HashSet<Reserva>();
    }
}
