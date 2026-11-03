using HotelAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Data
{
    public class HotelContext : DbContext
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Hotel> Hoteles { get; set; }
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<ReservaDetalle> ReservaDetalles { get; set; }
        public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
                        
            modelBuilder.Entity<Habitacion>()
                .HasOne(h => h.Hotel)
                .WithMany(h => h.Habitaciones)
                .HasForeignKey(h => h.HotelId)
                .OnDelete(DeleteBehavior.Restrict);
                        
            modelBuilder.Entity<ReservaDetalle>()
                .HasOne(rd => rd.Reserva)
                .WithMany(r => r.Detalles)
                .HasForeignKey(rd => rd.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);
                        
            modelBuilder.Entity<Reserva>()
                .HasMany(r => r.Servicios)
                .WithMany(s => s.Reservas)
                .UsingEntity(j => j.ToTable("ReservaServicio"));
                        
            modelBuilder.Entity<Habitacion>().Property(h => h.Precio).HasPrecision(18, 2);
            modelBuilder.Entity<Reserva>().Property(r => r.Total).HasPrecision(18, 2);
            modelBuilder.Entity<ReservaDetalle>().Property(rd => rd.Subtotal).HasPrecision(18, 2);
            modelBuilder.Entity<ServicioAdicional>().Property(s => s.Precio).HasPrecision(18, 2);
        }
    }
}
