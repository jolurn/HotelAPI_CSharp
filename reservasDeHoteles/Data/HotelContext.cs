using HotelAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Data
{
    // ==========================================
    // EL "CEREBRO" DE LA BASE DE DATOS
    // ==========================================
    // Esta clase hereda de DbContext, lo que le da superpoderes para conectarse a SQL Server.
    // Es el equivalente a abrir una sesión en SQL Server Management Studio.
    public class HotelContext : DbContext
    {
        // El constructor recibe las "opciones" (que incluyen la cadena de conexión del appsettings.json)
        // y se las pasa al padre (base) para que sepa a qué servidor conectarse.
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        // ==========================================
        // LAS TABLAS (DbSet)
        // ==========================================
        // Cada DbSet representa una tabla física en SQL Server.
        // Cuando el ORM se ejecute, creará estas 6 tablas automáticamente.
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Hotel> Hoteles { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ReservaDetalle> ReservaDetalles { get; set; }
        public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }

        // ==========================================
        // REGLAS DE NEGOCIO Y RELACIONES (Fluent API)
        // ==========================================
        // Este método es donde configuramos las relaciones "complejas" que no se pueden
        // poner con simples atributos en las clases (como [ForeignKey]).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------------------------------------------
            // RELACIÓN 1: UN HOTEL TIENE MUCHAS HABITACIONES (1 a N)
            // -------------------------------------------------------------------            
            modelBuilder.Entity<Habitacion>()
                .HasOne(h => h.Hotel)// 1. Una habitación tiene UN Hotel
                .WithMany(h => h.Habitaciones)// 2. Ese Hotel tiene MUCHAS Habitaciones
                .HasForeignKey(h => h.HotelId)// 3. La llave foránea es HotelId
                .OnDelete(DeleteBehavior.Restrict);// 4. REGLA: No permitas borrar un Hotel si tiene habitaciones (protege los datos).

            // -------------------------------------------------------------------
            // RELACIÓN 2: UNA RESERVA TIENE MUCHOS DETALLES (1 a N)
            // -------------------------------------------------------------------
            modelBuilder.Entity<ReservaDetalle>()
                .HasOne(rd => rd.Reserva)// 1. Un detalle pertenece a UNA Reserva
                .WithMany(r => r.Detalles)// 2. Esa Reserva tiene MUCHOS Detalles
                .HasForeignKey(rd => rd.ReservaId)// 3. La llave foránea es ReservaId
                .OnDelete(DeleteBehavior.Cascade);// 4. REGLA: Si borras la Reserva principal, borra sus detalles en cascada.

            // -------------------------------------------------------------------
            // RELACIÓN 3: MUCHOS A MUCHOS (Reserva <-> ServiciosAdicionales)
            // -------------------------------------------------------------------
            // Una reserva puede tener muchos servicios, y un servicio puede estar en muchas reservas.
            // Como SQL Server no permite esto directamente, EF Core crea una tabla intermedia oculta.
            modelBuilder.Entity<Reserva>()
                .HasMany(r => r.Servicios)
                .WithMany(s => s.Reservas)
                .UsingEntity(j => j.ToTable("ReservaServicio"));// Aquí le damos el nombre a la tabla intermedia.

            // -------------------------------------------------------------------
            // CONFIGURACIÓN DE DECIMALES (Para que no se pierdan los centavos)
            // -------------------------------------------------------------------
            // Sin esto, SQL Server no sabe cuántos decimales guardar y lanza advertencias.
            // HasPrecision(18, 2) significa: 18 dígitos en total, 2 de ellos después de la coma.
            modelBuilder.Entity<Habitacion>().Property(h => h.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Reserva>().Property(r => r.Total).HasPrecision(18, 2);
            modelBuilder.Entity<ReservaDetalle>().Property(rd => rd.Subtotal).HasPrecision(18, 2);
            modelBuilder.Entity<ServicioAdicional>().Property(s => s.Precio).HasPrecision(18, 2);
        }
    }
}
