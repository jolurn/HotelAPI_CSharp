namespace HotelAPI.DTOs
{
    public class ReservaLecturaDTO
    {
        // ==========================================
        // DTO DE SALIDA (Lo que el servidor DEVUELVE)
        // ==========================================
        // Este DTO se usa en el GET. Su función es aplanar los datos y entregar
        // un JSON limpio, sin ciclos infinitos ni datos innecesarios.
        public int Id { get; set; }
        public DateTime FechaReserva { get; set; }
        public decimal Total { get; set; }
        // 👇 Aplanamiento de datos (Flat) 👇
        // En lugar de devolver el objeto 'Usuario' completo (que traería su lista de reservas
        // y crearía un ciclo infinito), solo devolvemos el 'string' con su nombre.
        public string NombreUsuario { get; set; } = string.Empty;

        // En lugar de devolver la lista entera de 'ReservaDetalles', 
        // solo devolvemos un número entero con la cantidad.
        public int CantidadHabitaciones { get; set; }
    }
}
