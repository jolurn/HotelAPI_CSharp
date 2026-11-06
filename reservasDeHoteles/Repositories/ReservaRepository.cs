using Dapper;
using HotelAPI.Data;
using HotelAPI.DTOs;
using HotelAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Repositories
{
    // ==========================================
    // EL REPOSITORIO (Arquitectura Híbrida: Dapper + EF Core)
    // ==========================================
    // Aquí aplicamos CQRS Lite: Lecturas con Dapper (rápido) y Escrituras con EF Core (seguro).
    public class ReservaRepository
    {
        private readonly HotelContext _context;// Para escritura (EF Core)
        private readonly string _connectionString;// Para lectura (Dapper)

        public ReservaRepository(HotelContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        // ==========================================
        // 1. LECTURA (GET) CON DAPPER
        // ==========================================
        public async Task<IEnumerable<ReservaLecturaDTO>> ObtenerTodasAsync()
        {
            // Dapper abre una conexión directa y ligera a SQL Server.
            using var connection = new SqlConnection(_connectionString);

            // Hacemos un INNER JOIN para traer el nombre del usuario en una sola consulta.
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

            // Dapper mapea el resultado directamente a nuestro DTO de lectura.
            return await connection.QueryAsync<ReservaLecturaDTO>(sql);
        }

        // ==========================================
        // 2. ESCRITURA (POST) CON EF CORE Y TRANSACCIÓN
        // ==========================================
        public async Task<int> CrearReservaAsync(CrearReservaDTO dto)
        {
            // PASO A: Abrimos la "bóveda" (Transacción). Nada se guarda hasta el Commit.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalCalculado = 0;
                var nuevaReserva = new Reserva
                {
                    UsuarioId = dto.UsuarioId,
                    FechaReserva = DateTime.Now
                };

                // PASO B: Recorremos las habitaciones que envió el usuario en el DTO.
                foreach (var item in dto.Detalles)
                {
                    // Buscamos la habitación en la BD para obtener su Precio real (evita hackeos).
                    var habitacion = await _context.Habitaciones.FindAsync(item.HabitacionId);
                    if (habitacion == null)
                        throw new Exception($"La habitación con ID {item.HabitacionId} no existe.");

                    // Calculamos el subtotal en el servidor (Precio * Noches).
                    decimal subtotal = habitacion.Precio * item.Noches;
                    totalCalculado += subtotal;

                    // Agregamos el detalle al HashSet de la reserva en memoria.
                    nuevaReserva.Detalles.Add(new ReservaDetalle
                    {
                        HabitacionId = item.HabitacionId,
                        Noches = item.Noches,
                        Subtotal = subtotal
                    });
                }

                // PASO C: Procesamos los servicios adicionales (Relación N:M).
                if (dto.ServiciosAdicionalesIds != null && dto.ServiciosAdicionalesIds.Any())
                {
                    var servicios = await _context.ServiciosAdicionales
                        .Where(s => dto.ServiciosAdicionalesIds.Contains(s.Id))
                        .ToListAsync();

                    foreach (var servicio in servicios)
                    {
                        totalCalculado += servicio.Precio;
                        nuevaReserva.Servicios.Add(servicio);// EF Core llenará la tabla intermedia
                    }
                }

                // PASO D: Asignamos el Total final y preparamos el INSERT.
                nuevaReserva.Total = totalCalculado;
                _context.Reservas.Add(nuevaReserva);

                // PASO E: Guardamos los cambios. (Aquí se ejecutan los INSERT reales).
                await _context.SaveChangesAsync();

                // PASO F: Si todo salió bien, confirmamos la transacción.
                await transaction.CommitAsync();

                return nuevaReserva.Id;
            }
            catch (Exception)
            {
                // PASO G: ¡ERROR! Si algo falló (ej. la habitación no existía), 
                // deshacemos TODO. Ningún dato a medias se queda en la BD (Rollback).
                await transaction.RollbackAsync();
                throw;// Lanzamos el error para que el Controlador lo atrape.
            }
        }

        // ==========================================
        // 3. ACTUALIZAR Y ELIMINAR (EF Core)
        // ==========================================
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

            // El Cascade que configuramos en el DbContext borrará los ReservaDetalles automáticamente.
            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==========================================
        // 4. REPORTE DETALLADO (Voucher con Dapper)
        // ==========================================
        public async Task<IEnumerable<ReservaCompletaDTO>> ObtenerReporteCompletoAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // ¿Qué es STUFF y FOR XML PATH? 
            // Es un "truco" de SQL Server para concatenar filas en una sola cadena de texto.
            // Por ejemplo, si una reserva tiene las habitaciones "101" y "102", 
            // el resultado será: "101, 102" en una sola celda.
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

            // Usamos 'dynamic' porque el resultado trae strings separados por comas que debemos procesar.
            var resultado = await connection.QueryAsync<dynamic>(sql);

            // Como Dapper trae los strings separados por comas, los convertimos a Listas
            var listaFinal = new List<ReservaCompletaDTO>();
            foreach (var item in resultado)
            {
                // Convertimos a string de forma segura por si viene nulo.
                string habitacionesStr = item.HabitacionesReservadas?.ToString() ?? "";
                string serviciosStr = item.ServiciosAdicionales?.ToString() ?? "";

                listaFinal.Add(new ReservaCompletaDTO
                {
                    IdReserva = item.IdReserva,
                    NombreUsuario = item.NombreUsuario,
                    EmailUsuario = item.EmailUsuario,
                    FechaReserva = item.FechaReserva,
                    Total = item.Total,

                    // Hacemos un Split (cortamos el texto) usando la coma y el espacio como separador.
                    // Así convertimos "101, 102" en una lista real de C#: ["101", "102"].
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
