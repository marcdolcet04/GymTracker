using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Models;

namespace GymTracker.Data.DAO
{
    public class SesionDAO : BaseDAO<Sesion>
    {
        public override List<Sesion> GetAll()
        {
            try
            {
                var lista = new List<Sesion>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "SELECT * FROM Sesion ORDER BY Fecha DESC", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        public override Sesion GetById(int id)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "SELECT * FROM Sesion WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                using var reader = command.ExecuteReader();
                return reader.Read() ? Mapear(reader) : null;
            }
            catch (Exception) { return null; }
        }

        public override bool Insert(Sesion entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    INSERT INTO Sesion (RutinaId, Fecha, DuracionMinutos, Notas)
                    VALUES (@RutinaId, @Fecha, @DuracionMinutos, @Notas)", connection);
                command.Parameters.AddWithValue("@RutinaId",        entity.RutinaId);
                command.Parameters.AddWithValue("@Fecha",           entity.Fecha.ToString("o"));
                command.Parameters.AddWithValue("@DuracionMinutos", entity.DuracionMinutos);
                command.Parameters.AddWithValue("@Notas",           entity.Notas ?? (object)DBNull.Value);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public override bool Update(Sesion entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    UPDATE Sesion
                    SET RutinaId = @RutinaId, Fecha = @Fecha,
                        DuracionMinutos = @DuracionMinutos, Notas = @Notas
                    WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@RutinaId",        entity.RutinaId);
                command.Parameters.AddWithValue("@Fecha",           entity.Fecha.ToString("o"));
                command.Parameters.AddWithValue("@DuracionMinutos", entity.DuracionMinutos);
                command.Parameters.AddWithValue("@Notas",           entity.Notas ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Id",              entity.Id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public override bool Delete(int id)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "DELETE FROM Sesion WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public List<Sesion> GetByFecha(DateTime desde, DateTime hasta)
        {
            try
            {
                var lista = new List<Sesion>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT * FROM Sesion
                    WHERE Fecha BETWEEN @Desde AND @Hasta
                    ORDER BY Fecha DESC", connection);
                command.Parameters.AddWithValue("@Desde", desde.ToString("o"));
                command.Parameters.AddWithValue("@Hasta", hasta.ToString("o"));
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        private Sesion Mapear(SqliteDataReader reader) => new Sesion
        {
            Id              = reader.GetInt32(reader.GetOrdinal("Id")),
            RutinaId        = reader.GetInt32(reader.GetOrdinal("RutinaId")),
            Fecha           = DateTime.Parse(reader.GetString(reader.GetOrdinal("Fecha"))),
            DuracionMinutos = reader.GetInt32(reader.GetOrdinal("DuracionMinutos")),
            Notas           = reader.IsDBNull(reader.GetOrdinal("Notas")) ? null : reader.GetString(reader.GetOrdinal("Notas"))
        };
    }
}
