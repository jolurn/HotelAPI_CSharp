namespace HotelAPI.Models
{
    // ==========================================
    // EL MODELO TRANSACCIONAL (EL "CARRITO")
    // ==========================================
    // Esta entidad representa la cabecera de la reserva. 
    // Es el equivalente a la tabla 'Reservas' en SQL Server.
    public class Reserva
    {
        // Llave primaria.
        public int Id { get; set; }

        // Fecha en la que se hace la reserva. Se inicializa automáticamente con la fecha actual.
        public DateTime FechaReserva { get; set; } = DateTime.Now;

        // El monto total a pagar. Este valor NO lo pone el usuario desde Swagger,
        // se calcula automáticamente en el Repositorio sumando habitaciones y servicios.
        public decimal Total { get; set; }

        // ==========================================
        // RELACIÓN 1: USUARIO (1 a N)
        // ==========================================
        // Llave foránea: Guarda el ID del usuario que hizo la reserva.
        public int UsuarioId { get; set; }

        // Propiedad de navegación: Permite acceder a los datos del Usuario (Nombre, Email)
        // desde la reserva. El '?' significa que puede ser nulo si no se carga con Include.
        public Usuario? Usuario { get; set; }

        // ==========================================
        // RELACIÓN 2: DETALLES (1 a N) - LO QUE EL PROFE PIDIÓ
        // ==========================================
        // Usamos un HashSet en lugar de un List. ¿Por qué?
        // Porque el HashSet NO permite elementos duplicados. 
        // Si por error intentas agregar la misma habitación dos veces en la misma reserva,
        // el HashSet lo ignorará, protegiendo la lógica de negocio en memoria.
        public ICollection<ReservaDetalle> Detalles { get; set; } = new HashSet<ReservaDetalle>();

        // ==========================================
        // RELACIÓN 3: SERVICIOS (N a M)
        // ==========================================
        // Una reserva puede tener muchos servicios (Spa, Desayuno). 
        // Entity Framework usará esto (junto con el HotelContext) para llenar la tabla intermedia.
        public ICollection<ServicioAdicional> Servicios { get; set; } = new HashSet<ServicioAdicional>();
    }
}
