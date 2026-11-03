namespace HotelAPI.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public List<Habitacion> Habitaciones { get; set; } = new();
    }
}
