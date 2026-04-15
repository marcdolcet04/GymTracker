using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymTracker.Data.DAO;
using GymTracker.Models;
using GymTracker.Services;

namespace GymTracker.Views
{
    public partial class SesionView : UserControl
    {
        private readonly RutinaDAO     _rutinaDao     = new();
        private readonly EjercicioDAO  _ejercicioDao  = new();
        private readonly SesionService _sesionService = new();

        // ViewModel local para las series de la sesión en curso
        private class SerieEntrada
        {
            public int    EjercicioId     { get; set; }
            public string NombreEjercicio { get; set; }
            public int    NumSerie        { get; set; }
            public int    Repeticiones    { get; set; }
            public double PesoKg          { get; set; }
        }

        private ObservableCollection<SerieEntrada> _series = new();
        private List<Ejercicio> _ejerciciosRutina = new();

        public SesionView()
        {
            InitializeComponent();
            CargarRutinas();
            DpFecha.SelectedDate = DateTime.Today;
            GridSeries.ItemsSource = _series;
        }

        // ──────────────────────────────────────────────────────────────────
        // Carga de datos
        // ──────────────────────────────────────────────────────────────────

        private void CargarRutinas()
        {
            CmbRutina.ItemsSource = _rutinaDao.GetAll() ?? new List<Rutina>();
        }

        private void CargarEjerciciosDeRutina(int rutinaId)
        {
            _ejerciciosRutina.Clear();

            var asignados = _rutinaDao.GetEjerciciosByRutina(rutinaId) ?? new List<RutinaEjercicio>();
            var idsAsignados = asignados.Select(re => re.EjercicioId).Distinct().ToHashSet();
            var todos = _ejercicioDao.GetAll() ?? new List<Ejercicio>();

            foreach (var ejercicio in todos.Where(e => idsAsignados.Contains(e.Id)).OrderBy(e => e.Nombre))
                _ejerciciosRutina.Add(ejercicio);

            CmbEjercicioSerie.ItemsSource = _ejerciciosRutina;
            if (_ejerciciosRutina.Any())
            {
                CmbEjercicioSerie.SelectedIndex = 0;
                TxtSinEjercicios.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtSinEjercicios.Visibility = Visibility.Visible;
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Eventos
        // ──────────────────────────────────────────────────────────────────

        private void CmbRutina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _series.Clear();
            var rutina = CmbRutina.SelectedItem as Rutina;
            if (rutina != null)
            {
                CargarEjerciciosDeRutina(rutina.Id);
            }
            else
            {
                _ejerciciosRutina.Clear();
                CmbEjercicioSerie.ItemsSource = null;
                TxtSinEjercicios.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnAnadirSerie_Click(object sender, RoutedEventArgs e)
        {
            if (CmbEjercicioSerie.SelectedItem is not Ejercicio ejercicio)
            {
                MessageBox.Show("Selecciona un ejercicio.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Calcular el número de serie para este ejercicio en la sesión actual
            int numSerie = _series.Count(s => s.EjercicioId == ejercicio.Id) + 1;

            _series.Add(new SerieEntrada
            {
                EjercicioId     = ejercicio.Id,
                NombreEjercicio = ejercicio.Nombre,
                NumSerie        = numSerie,
                Repeticiones    = 0,
                PesoKg          = 0
            });
        }

        private void BtnEliminarSerie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is SerieEntrada serie)
            {
                _series.Remove(serie);
                // Recalcular NumSerie por ejercicio
                foreach (var grupo in _series.GroupBy(s => s.EjercicioId))
                {
                    int i = 1;
                    foreach (var s in grupo)
                        s.NumSerie = i++;
                }
                GridSeries.Items.Refresh();
            }
        }

        private void BtnGuardarSesion_Click(object sender, RoutedEventArgs e)
        {
            // ── Validaciones ──
            if (CmbRutina.SelectedItem is not Rutina rutina)
            {
                MessageBox.Show("Selecciona una rutina.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DpFecha.SelectedDate == null)
            {
                MessageBox.Show("Selecciona una fecha.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!_series.Any())
            {
                MessageBox.Show("Añade al menos una serie antes de guardar.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ── Construir objetos ──
            var sesion = new Sesion
            {
                RutinaId        = rutina.Id,
                Fecha           = DpFecha.SelectedDate.Value,
                DuracionMinutos = 0,   // se puede ampliar con un campo de duración
                Notas           = TxtNotas.Text.Trim()
            };

            var seriesRegistro = _series.Select(s => new SerieRegistro
            {
                EjercicioId  = s.EjercicioId,
                NumSerie     = s.NumSerie,
                Repeticiones = s.Repeticiones,
                PesoKg       = s.PesoKg
            }).ToList();

            // ── Guardar en transacción ──
            bool ok = _sesionService.GuardarSesionCompleta(sesion, seriesRegistro);
            if (!ok)
            {
                MessageBox.Show("Error al guardar la sesión. Comprueba los datos.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Sesión guardada correctamente.",
                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpiar el formulario para nueva sesión
            _series.Clear();
            TxtNotas.Text        = string.Empty;
            DpFecha.SelectedDate = DateTime.Today;
            CmbRutina.SelectedIndex = -1;
        }
    }
}
