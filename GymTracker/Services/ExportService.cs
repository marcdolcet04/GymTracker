using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GymTracker.Models;

namespace GymTracker.Services
{
    public class ExportService
    {
        private const string Delimitador = ";";

        /// <summary>
        /// Exporta el historial de series a un fichero CSV con separador punto y coma.
        /// Cabecera: Fecha;Rutina;Ejercicio;Serie;Repeticiones;PesoKg
        /// Excel (configuración regional española) lo abre directamente sin importación manual.
        /// </summary>
        public bool ExportarCSV(List<SerieRegistroDetalle> datos, string rutaArchivo)
        {
            try
            {
                var sb = new StringBuilder();

                // Cabecera
                sb.AppendLine("Fecha;Rutina;Ejercicio;Serie;Repeticiones;PesoKg");

                // Filas de datos
                foreach (var serie in datos)
                {
                    sb.AppendLine(string.Join(Delimitador,
                        serie.FechaSesion.ToString("yyyy-MM-dd"),
                        EscaparCSV(serie.NombreRutina),
                        EscaparCSV(serie.NombreEjercicio),
                        serie.NumSerie,
                        serie.Repeticiones,
                        serie.PesoKg.ToString("F2")
                    ));
                }

                File.WriteAllText(rutaArchivo, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Envuelve en comillas los campos que contengan punto y coma o comillas.
        /// </summary>
        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor)) return string.Empty;
            if (valor.Contains(";") || valor.Contains("\""))
                return $"\"{valor.Replace("\"", "\"\"")}\"";
            return valor;
        }
    }
}
