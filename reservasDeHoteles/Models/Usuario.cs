namespace HotelAPI.Models
{
    // ==========================================
    // PASO 1: EL MODELO (LA ENTIDAD)
    // ==========================================
    // Esta clase representa exactamente la tabla 'Usuarios' en SQL Server.
    // Entity Framework la usará para crear las columnas y las relaciones.
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // 👇 NUEVO CAMPO: Aquí guardaremos la contraseña encriptada
        public string PasswordHash { get; set; } = string.Empty;

        // 👇 NUEVO CAMPO: Por defecto, un usuario nuevo NO está bloqueado.
        public bool EstaBloqueado { get; set; } = false;

        public List<Reserva> Reservas { get; set; } = new();
    }
}
