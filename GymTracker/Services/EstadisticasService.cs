using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymTracker.Data;

namespace GymTracker.Services
{
    public class EstadisticasService
    {
        /// <summary>
        /// Punto de datos para las gráficas de progresión.
        /// </summary>
        public class DataPoint
        {
            public DateTime Fecha { get; set; }
            public double Valor { get; set; }
        }

        /// <summary>
        /// Devuelve la progresión del peso máximo levantado por sesión para un ejercicio.
        /// Útil para ver si el usuario está progresando en carga.
        /// </summary>
        public List<DataPoint> GetProgresionPesoMaximo(int ejercicioId)
        {
            try
            {
                var lista = new List<DataPoint>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT s.Fecha, MAX(sr.PesoKg) AS Valor
                    FROM SerieRegistro sr
                    JOIN Sesion s ON sr.SesionId = s.Id
                    WHERE sr.EjercicioId = @EjercicioId
                    GROUP BY s.Id
                    ORDER BY s.Fecha", connection);

                command.Parameters.AddWithValue("@EjercicioId", ejercicioId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new DataPoint
                    {
                        Fecha = DateTime.Parse(reader.GetString(reader.GetOrdinal("Fecha"))),
                        Valor = reader.GetDouble(reader.GetOrdinal("Valor"))
                    });
                }

                return lista;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Devuelve el volumen total (suma de repeticiones × peso) por sesión para un ejercicio.
        /// Útil para medir la carga de trabajo acumulada.
        /// </summary>
        public List<DataPoint> GetVolumenTotal(int ejercicioId)
        {
            try
            {
                var lista = new List<DataPoint>();
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqliteCommand(@"
                    SELECT s.Fecha, SUM(sr.Repeticiones * sr.PesoKg) AS Valor
                    FROM SerieRegistro sr
                    JOIN Sesion s ON sr.SesionId = s.Id
                    WHERE sr.EjercicioId = @EjercicioId
                    GROUP BY s.Id
                    ORDER BY s.Fecha", connection);

                command.Parameters.AddWithValue("@EjercicioId", ejercicioId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new DataPoint
                    {
                        Fecha = DateTime.Parse(reader.GetString(reader.GetOrdinal("Fecha"))),
                        Valor = reader.GetDouble(reader.GetOrdinal("Valor"))
                    });
                }

                return lista;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
