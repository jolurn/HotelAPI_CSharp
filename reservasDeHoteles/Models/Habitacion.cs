namespace HotelAPI.Models
{
    public class Habitacion
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public decimal Precio { get; set; }

        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
    }
}
