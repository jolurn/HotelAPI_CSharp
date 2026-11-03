namespace HotelAPI.Models
{
    public class Reserva
    {
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; } = DateTime.Now;
        public decimal Total { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        
        public ICollection<ReservaDetalle> Detalles { get; set; } = new HashSet<ReservaDetalle>();

        
        public ICollection<ServicioAdicional> Servicios { get; set; } = new HashSet<ServicioAdicional>();
    }
}
