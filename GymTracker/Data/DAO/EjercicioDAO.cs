using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Models;

namespace GymTracker.Data.DAO
{
    public class EjercicioDAO : BaseDAO<Ejercicio>
    {
        public override List<Ejercicio> GetAll()
        {
            try
            {
                var lista = new List<Ejercicio>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand("SELECT * FROM Ejercicio", connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        public override Ejercicio GetById(int id)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "SELECT * FROM Ejercicio WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                using var reader = command.ExecuteReader();
                return reader.Read() ? Mapear(reader) : null;
            }
            catch (Exception) { return null; }
        }

        public override bool Insert(Ejercicio entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    INSERT INTO Ejercicio (Nombre, GrupoMuscular, Descripcion, ImagenRuta)
                    VALUES (@Nombre, @GrupoMuscular, @Descripcion, @ImagenRuta)", connection);
                command.Parameters.AddWithValue("@Nombre",        entity.Nombre);
                command.Parameters.AddWithValue("@GrupoMuscular", entity.GrupoMuscular);
                command.Parameters.AddWithValue("@Descripcion",   entity.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ImagenRuta",    entity.ImagenRuta  ?? (object)DBNull.Value);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public override bool Update(Ejercicio entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    UPDATE Ejercicio
                    SET Nombre = @Nombre, GrupoMuscular = @GrupoMuscular,
                        Descripcion = @Descripcion, ImagenRuta = @ImagenRuta
                    WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Nombre",        entity.Nombre);
                command.Parameters.AddWithValue("@GrupoMuscular", entity.GrupoMuscular);
                command.Parameters.AddWithValue("@Descripcion",   entity.Descripcion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@ImagenRuta",    entity.ImagenRuta  ?? (object)DBNull.Value);
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
                    "DELETE FROM Ejercicio WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public List<Ejercicio> GetByGrupoMuscular(string grupo)
        {
            try
            {
                var lista = new List<Ejercicio>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(
                    "SELECT * FROM Ejercicio WHERE GrupoMuscular = @Grupo", connection);
                command.Parameters.AddWithValue("@Grupo", grupo);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        private Ejercicio Mapear(SqliteDataReader reader) => new Ejercicio
        {
            Id            = reader.GetInt32(reader.GetOrdinal("Id")),
            Nombre        = reader.GetString(reader.GetOrdinal("Nombre")),
            GrupoMuscular = reader.GetString(reader.GetOrdinal("GrupoMuscular")),
            Descripcion   = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
            ImagenRuta    = reader.IsDBNull(reader.GetOrdinal("ImagenRuta"))  ? null : reader.GetString(reader.GetOrdinal("ImagenRuta"))
        };
    }
}
