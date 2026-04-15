using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Models;

namespace GymTracker.Data.DAO
{
    public class RutinaDAO : BaseDAO<Rutina>
    {
        public override List<Rutina> GetAll()
        {
            try
            {
                var lista = new List<Rutina>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand("SELECT * FROM Rutina", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        public override Rutina GetById(int id)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "SELECT * FROM Rutina WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                using var reader = command.ExecuteReader();
                return reader.Read() ? Mapear(reader) : null;
            }
            catch (Exception) { return null; }
        }

        public override bool Insert(Rutina entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    INSERT INTO Rutina (Nombre, Descripcion, FechaCreacion)
                    VALUES (@Nombre, @Descripcion, @FechaCreacion)", connection);
                command.Parameters.AddWithValue("@Nombre",        entity.Nombre);
                command.Parameters.AddWithValue("@Descripcion",   entity.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaCreacion", entity.FechaCreacion.ToString("o"));
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public override bool Update(Rutina entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    UPDATE Rutina
                    SET Nombre = @Nombre, Descripcion = @Descripcion, FechaCreacion = @FechaCreacion
                    WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Nombre",        entity.Nombre);
                command.Parameters.AddWithValue("@Descripcion",   entity.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaCreacion", entity.FechaCreacion.ToString("o"));
                command.Parameters.AddWithValue("@Id",            entity.Id);
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
                    "DELETE FROM Rutina WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public List<RutinaEjercicio> GetEjerciciosByRutina(int rutinaId)
        {
            try
            {
                var lista = new List<RutinaEjercicio>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT * FROM RutinaEjercicio
                    WHERE RutinaId = @RutinaId
                    ORDER BY DiaSemana, Orden", connection);
                command.Parameters.AddWithValue("@RutinaId", rutinaId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(MapearRutinaEjercicio(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        private Rutina Mapear(SqliteDataReader reader) => new Rutina
        {
            Id            = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre        = reader.GetString(reader.GetOrdinal("Nombre")),
            Descripcion   = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
            FechaCreacion = DateTime.Parse(reader.GetString(reader.GetOrdinal("FechaCreacion")))
        };

        private RutinaEjercicio MapearRutinaEjercicio(SqliteDataReader reader) => new RutinaEjercicio
        {
            Id          = reader.GetInt32(reader.GetOrdinal("Id")),
            RutinaId    = reader.GetInt32(reader.GetOrdinal("RutinaId")),
            EjercicioId = reader.GetInt32(reader.GetOrdinal("EjercicioId")),
            DiaSemana   = reader.GetString(reader.GetOrdinal("DiaSemana")),
            Orden       = reader.GetInt32(reader.GetOrdinal("Orden"))
        };
    }
}
