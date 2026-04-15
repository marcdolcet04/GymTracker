using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services
{
    public class HistorialService
    {
        /// <summary>
        /// Devuelve el historial de series aplicando los filtros que no sean null.
        /// Incluye nombre del ejercicio, nombre de la rutina y fecha de la sesión.
        /// </summary>
        public List<SerieRegistroDetalle> GetHistorialFiltrado(
            DateTime? desde, DateTime? hasta, int? ejercicioId)
        {
            try
            {
                var lista = new List<SerieRegistroDetalle>();
                using var connection = DatabaseHelper.GetConnection();

                // Construir la consulta con los filtros opcionales
                var sql = @"
                    SELECT sr.Id, sr.SesionId, sr.EjercicioId, sr.NumSerie,
                           sr.Repeticiones, sr.PesoKg,
                           e.Nombre  AS NombreEjercicio,
                           r.Nombre  AS NombreRutina,
                           s.Fecha   AS FechaSesion
                    FROM SerieRegistro sr
                    JOIN Ejercicio e ON sr.EjercicioId = e.Id
                    JOIN Sesion    s ON sr.SesionId    = s.Id
                    JOIN Rutina    r ON s.RutinaId     = r.Id
                    WHERE (@Desde       IS NULL OR s.Fecha        >= @Desde)
                      AND (@Hasta       IS NULL OR s.Fecha        <= @Hasta)
                      AND (@EjercicioId IS NULL OR sr.EjercicioId  = @EjercicioId)
                    ORDER BY s.Fecha DESC, sr.EjercicioId, sr.NumSerie";

                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@Desde",       (object)desde?.ToString("o")  ?? DBNull.Value);
                command.Parameters.AddWithValue("@Hasta",       (object)hasta?.ToString("o")  ?? DBNull.Value);
                command.Parameters.AddWithValue("@EjercicioId", (object)ejercicioId           ?? DBNull.Value);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));

                return lista;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private SerieRegistroDetalle Mapear(SqliteDataReader reader) => new SerieRegistroDetalle
        {
            Id              = reader.GetInt32(reader.GetOrdinal("Id")),
            SesionId        = reader.GetInt32(reader.GetOrdinal("SesionId")),
            EjercicioId     = reader.GetInt32(reader.GetOrdinal("EjercicioId")),
            NumSerie        = reader.GetInt32(reader.GetOrdinal("NumSerie")),
            Repeticiones    = reader.GetInt32(reader.GetOrdinal("Repeticiones")),
            PesoKg          = reader.GetDouble(reader.GetOrdinal("PesoKg")),
            NombreEjercicio = reader.GetString(reader.GetOrdinal("NombreEjercicio")),
            NombreRutina    = reader.GetString(reader.GetOrdinal("NombreRutina")),
            FechaSesion     = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaSesion")))
        };
    }
}
