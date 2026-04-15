using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Data;
using GymTracker.Models;

namespace GymTracker.Services
{
    public class SesionService
    {
        /// <summary>
        /// Guarda la sesión y todas sus series en una única transacción.
        /// Si falla cualquier inserción se hace rollback completo.
        /// </summary>
        public bool GuardarSesionCompleta(Sesion sesion, List<SerieRegistro> series)
        {
            using var connection = DatabaseHelper.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insertar la sesión y obtener su ID generado
                int sesionId = InsertarSesion(sesion, connection, transaction);

                // Insertar cada serie asociada a la sesión recién creada
                foreach (var serie in series)
                {
                    serie.SesionId = sesionId;
                    InsertarSerie(serie, connection, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
        }

        /// <summary>
        /// Elimina la sesión y sus series en cascada (ON DELETE CASCADE en la BD).
        /// </summary>
        public bool EliminarSesion(int sesionId)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "DELETE FROM Sesion WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", sesionId);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private int InsertarSesion(Sesion sesion, SqliteConnection connection, SqliteTransaction transaction)
        {
            using var command = new SqliteCommand(@"
                INSERT INTO Sesion (RutinaId, Fecha, DuracionMinutos, Notas)
                VALUES (@RutinaId, @Fecha, @DuracionMinutos, @Notas);
                SELECT last_insert_rowid();", connection, transaction);

            command.Parameters.AddWithValue("@RutinaId", sesion.RutinaId);
            command.Parameters.AddWithValue("@Fecha", sesion.Fecha.ToString("o"));
            command.Parameters.AddWithValue("@DuracionMinutos", sesion.DuracionMinutos);
            command.Parameters.AddWithValue("@Notas", sesion.Notas ?? (object)DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private void InsertarSerie(SerieRegistro serie, SqliteConnection connection, SqliteTransaction transaction)
        {
            using var command = new SqliteCommand(@"
                INSERT INTO SerieRegistro (SesionId, EjercicioId, NumSerie, Repeticiones, PesoKg)
                VALUES (@SesionId, @EjercicioId, @NumSerie, @Repeticiones, @PesoKg)", connection, transaction);

            command.Parameters.AddWithValue("@SesionId", serie.SesionId);
            command.Parameters.AddWithValue("@EjercicioId", serie.EjercicioId);
            command.Parameters.AddWithValue("@NumSerie", serie.NumSerie);
            command.Parameters.AddWithValue("@Repeticiones", serie.Repeticiones);
            command.Parameters.AddWithValue("@PesoKg", serie.PesoKg);

            command.ExecuteNonQuery();
        }
    }
}
