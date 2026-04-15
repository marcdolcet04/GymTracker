using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymTracker.Data.DAO;
using GymTracker.Models;
using GymTracker.Services;
using Microsoft.Win32;

namespace GymTracker.Views
{
    public partial class HistorialView : UserControl
    {
        private readonly HistorialService _historialService = new();
        private readonly EjercicioDAO    _ejercicioDao     = new();
        private readonly ExportService   _exportService    = new();
        private readonly SesionService   _sesionService    = new();

        // Elemento especial "Todos los ejercicios" en el ComboBox
        private readonly Ejercicio _opcionTodos = new() { Id = 0, Nombre = "Todos los ejercicios" };

        private List<SerieRegistroDetalle> _datosActuales = new();

        public HistorialView()
        {
            InitializeComponent();
            InicializarFiltros();
            Cargar();
        }

        // ──────────────────────────────────────────────────────────────────
        // Inicialización
        // ──────────────────────────────────────────────────────────────────

        private void InicializarFiltros()
        {
            // Fechas por defecto: últimos 90 días
            DpDesde.SelectedDate = DateTime.Today.AddDays(-90);
            DpHasta.SelectedDate = DateTime.Today;

            // ComboBox ejercicios
            var ejercicios = _ejercicioDao.GetAll() ?? new List<Ejercicio>();
            ejercicios.Insert(0, _opcionTodos);
            CmbEjercicio.ItemsSource  = ejercicios;
            CmbEjercicio.SelectedIndex = 0;
        }

        // ──────────────────────────────────────────────────────────────────
        // Carga y filtrado
        // ──────────────────────────────────────────────────────────────────

        private void Cargar()
        {
            DateTime? desde      = DpDesde.SelectedDate;
            DateTime? hasta      = DpHasta.SelectedDate?.AddHours(23).AddMinutes(59);
            int?      ejercicioId = null;

            if (CmbEjercicio.SelectedItem is Ejercicio e && e.Id != 0)
                ejercicioId = e.Id;

            _datosActuales = _historialService.GetHistorialFiltrado(desde, hasta, ejercicioId)
                             ?? new List<SerieRegistroDetalle>();

            // Marcar la primera fila de cada sesión para mostrar el botón de eliminar
            var sesionesVistas = new HashSet<int>();
            foreach (var item in _datosActuales)
                item.EsPrimeraFilaDeSesion = sesionesVistas.Add(item.SesionId);

            GridHistorial.ItemsSource = _datosActuales;
            TxtContador.Text = $"{_datosActuales.Count} registros encontrados";
        }

        // ──────────────────────────────────────────────────────────────────
        // Eventos
        // ──────────────────────────────────────────────────────────────────

        private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
            => Cargar();

        private void BtnEliminarSesion_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SerieRegistroDetalle detalle) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar esta sesión del {detalle.FechaSesion:dd/MM/yyyy} con la rutina \"{detalle.NombreRutina}\"? Se eliminarán todas sus series.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            if (!_sesionService.EliminarSesion(detalle.SesionId))
            {
                MessageBox.Show("Error al eliminar la sesión.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Cargar();
        }

        private void BtnExportarCSV_Click(object sender, RoutedEventArgs e)
        {
            if (!_datosActuales.Any())
            {
                MessageBox.Show("No hay datos para exportar. Aplica un filtro primero.",
                    "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title      = "Guardar historial como CSV",
                Filter     = "Archivos CSV (*.csv)|*.csv",
                FileName   = $"historial_{DateTime.Today:yyyyMMdd}.csv",
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true) return;

            bool ok = _exportService.ExportarCSV(_datosActuales, dialog.FileName);

            if (ok)
                MessageBox.Show($"Archivo exportado correctamente:\n{dialog.FileName}",
                    "Exportación completada", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Error al exportar el archivo. Comprueba que no esté abierto.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
