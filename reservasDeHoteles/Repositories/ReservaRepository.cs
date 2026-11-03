using Dapper;
using HotelAPI.Data;
using HotelAPI.DTOs;
using HotelAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Repositories
{
    public class ReservaRepository
    {
        private readonly HotelContext _context;
        private readonly string _connectionString;

        public ReservaRepository(HotelContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }
                
        public async Task<IEnumerable<ReservaLecturaDTO>> ObtenerTodasAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    r.Id, 
                    r.FechaReserva, 
                    r.Total, 
                    u.Nombre AS NombreUsuario,
                    (SELECT COUNT(*) FROM ReservaDetalles rd WHERE rd.ReservaId = r.Id) AS CantidadHabitaciones
                FROM Reservas r
                INNER JOIN Usuarios u ON r.UsuarioId = u.Id
                ORDER BY r.FechaReserva DESC";

            return await connection.QueryAsync<ReservaLecturaDTO>(sql);
        }
                
        public async Task<int> CrearReservaAsync(CrearReservaDTO dto)
        {            
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalCalculado = 0;
                var nuevaReserva = new Reserva
                {
                    UsuarioId = dto.UsuarioId,
                    FechaReserva = DateTime.Now
                };
                                
                foreach (var item in dto.Detalles)
                {
                    var habitacion = await _context.Habitaciones.FindAsync(item.HabitacionId);
                    if (habitacion == null)
                        throw new Exception($"La habitación con ID {item.HabitacionId} no existe.");

                    decimal subtotal = habitacion.Precio * item.Noches;
                    totalCalculado += subtotal;
                                        
                    nuevaReserva.Detalles.Add(new ReservaDetalle
                    {
                        HabitacionId = item.HabitacionId,
                        Noches = item.Noches,
                        Subtotal = subtotal
                    });
                }
                                
                if (dto.ServiciosAdicionalesIds != null && dto.ServiciosAdicionalesIds.Any())
                {
                    var servicios = await _context.ServiciosAdicionales
                        .Where(s => dto.ServiciosAdicionalesIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var servicio in servicios)
                    {
                        totalCalculado += servicio.Precio;
                        nuevaReserva.Servicios.Add(servicio);
                    }
                }

                nuevaReserva.Total = totalCalculado;
                _context.Reservas.Add(nuevaReserva);
                                
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return nuevaReserva.Id;
            }
            catch (Exception)
            {                
                await transaction.RollbackAsync();
                throw;
            }
        }
               
        public async Task<bool> ActualizarAsync(Reserva reserva)
        {            
            if (!await _context.Reservas.AnyAsync(r => r.Id == reserva.Id))
                return false;

            _context.Entry(reserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return false;
                        
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return true;
        }

        // GET: Reporte detallado de reservas (Usando Dapper)
        public async Task<IEnumerable<ReservaCompletaDTO>> ObtenerReporteCompletoAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // Usamos un GROUP_CONCAT simulado en SQL Server para agrupar textos
            var sql = @"
                SELECT 
                    r.Id AS IdReserva,
                    u.Nombre AS NombreUsuario,
                    u.Email AS EmailUsuario,
                    r.FechaReserva,
                    r.Total,
                    -- Agrupamos las habitaciones en una sola cadena de texto (separadas por coma)
                    STUFF((
                        SELECT DISTINCT ', ' + h.Numero 
                        FROM ReservaDetalles rd
                        INNER JOIN Habitaciones h ON rd.HabitacionId = h.Id
                        WHERE rd.ReservaId = r.Id
                        FOR XML PATH('')
                    ), 1, 2, '') AS HabitacionesReservadas,
                    -- Agrupamos los servicios en una sola cadena de texto
                    STUFF((
                        SELECT DISTINCT ', ' + sa.Nombre 
                        FROM ReservaServicio rs
                        INNER JOIN ServiciosAdicionales sa ON rs.ServiciosId = sa.Id
                        WHERE rs.ReservasId = r.Id -- 👈 AQUÍ ESTÁ EL CAMBIO (agregamos la 's')
                        FOR XML PATH('')
                    ), 1, 2, '') AS ServiciosAdicionales
                FROM Reservas r
                INNER JOIN Usuarios u ON r.UsuarioId = u.Id
                ORDER BY r.FechaReserva DESC";

            // Dapper nos devuelve los datos planos
            var resultado = await connection.QueryAsync<dynamic>(sql);

            // Como Dapper trae los strings separados por comas, los convertimos a Listas
            var listaFinal = new List<ReservaCompletaDTO>();
            foreach (var item in resultado)
            {
                // Convertimos a string por si Dapper devuelve un array
                string habitacionesStr = item.HabitacionesReservadas?.ToString() ?? "";
                string serviciosStr = item.ServiciosAdicionales?.ToString() ?? "";

                listaFinal.Add(new ReservaCompletaDTO
                {
                    IdReserva = item.IdReserva,
                    NombreUsuario = item.NombreUsuario,
                    EmailUsuario = item.EmailUsuario,
                    FechaReserva = item.FechaReserva,
                    Total = item.Total,

                    // Ahora hacemos el split de forma segura
                    HabitacionesReservadas = string.IsNullOrEmpty(habitacionesStr)
                        ? new List<string>()
                        : habitacionesStr.Split(new[] { ", " }, StringSplitOptions.None).ToList(),

                    ServiciosAdicionales = string.IsNullOrEmpty(serviciosStr)
                        ? new List<string>()
                        : serviciosStr.Split(new[] { ", " }, StringSplitOptions.None).ToList()
                });
            }

            return listaFinal;
        }
    }
}
