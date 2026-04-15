using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Models;

namespace GymTracker.Data.DAO
{
    public class SerieRegistroDAO : BaseDAO<SerieRegistro>
    {
        public override List<SerieRegistro> GetAll()  => throw new NotImplementedException();
        public override SerieRegistro GetById(int id) => throw new NotImplementedException();
        public override bool Update(SerieRegistro entity) => throw new NotImplementedException();

        public override bool Insert(SerieRegistro entity)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    INSERT INTO SerieRegistro (SesionId, EjercicioId, NumSerie, Repeticiones, PesoKg)
                    VALUES (@SesionId, @EjercicioId, @NumSerie, @Repeticiones, @PesoKg)", connection);
                command.Parameters.AddWithValue("@SesionId",     entity.SesionId);
                command.Parameters.AddWithValue("@EjercicioId",  entity.EjercicioId);
                command.Parameters.AddWithValue("@NumSerie",     entity.NumSerie);
                command.Parameters.AddWithValue("@Repeticiones", entity.Repeticiones);
                command.Parameters.AddWithValue("@PesoKg",       entity.PesoKg);
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
                    "DELETE FROM SerieRegistro WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception) { return false; }
        }

        public List<SerieRegistro> GetBySesion(int sesionId)
        {
            try
            {
                var lista = new List<SerieRegistro>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT * FROM SerieRegistro
                    WHERE SesionId = @SesionId
                    ORDER BY EjercicioId, NumSerie", connection);
                command.Parameters.AddWithValue("@SesionId", sesionId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        public List<SerieRegistro> GetByEjercicio(int ejercicioId)
        {
            try
            {
                var lista = new List<SerieRegistro>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT * FROM SerieRegistro
                    WHERE EjercicioId = @EjercicioId
                    ORDER BY SesionId, NumSerie", connection);
                command.Parameters.AddWithValue("@EjercicioId", ejercicioId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    lista.Add(Mapear(reader));
                return lista;
            }
            catch (Exception) { return null; }
        }

        private SerieRegistro Mapear(SqliteDataReader reader) => new SerieRegistro
        {
            Id           = reader.GetInt32(reader.GetOrdinal("Id")),
            SesionId     = reader.GetInt32(reader.GetOrdinal("SesionId")),
            EjercicioId  = reader.GetInt32(reader.GetOrdinal("EjercicioId")),
            NumSerie     = reader.GetInt32(reader.GetOrdinal("NumSerie")),
            Repeticiones = reader.GetInt32(reader.GetOrdinal("Repeticiones")),
            PesoKg       = reader.GetDouble(reader.GetOrdinal("PesoKg"))
        };
    }
}
