using System;

namespace GymTracker.Models
{
    /// <summary>
    /// DTO que extiende SerieRegistro con datos de contexto
    /// necesarios para mostrar el historial y exportar a CSV.
    /// </summary>
    public class SerieRegistroDetalle
    {
        public int Id { get; set; }
        public int SesionId { get; set; }
        public int EjercicioId { get; set; }
        public int NumSerie { get; set; }
        public int Repeticiones { get; set; }
        public double PesoKg { get; set; }

        // Datos de contexto obtenidos por JOIN
        public string   NombreEjercicio   { get; set; }
        public string   NombreRutina      { get; set; }
        public DateTime FechaSesion       { get; set; }

        // UI helper: true solo en la primera fila de cada sesión
        public bool EsPrimeraFilaDeSesion { get; set; }
    }
}
