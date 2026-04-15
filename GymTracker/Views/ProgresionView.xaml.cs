using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymTracker.Data;
using GymTracker.Data.DAO;
using GymTracker.Models;
using GymTracker.Services;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;

namespace GymTracker.Views
{
    public partial class ProgresionView : UserControl
    {
        private readonly EjercicioDAO        _ejercicioDao        = new();
        private readonly EstadisticasService  _estadisticasService = new();

        // Gráfica creada en code-behind para evitar problemas del compilador XAML con LiveCharts
        private readonly CartesianChart _grafica = new()
        {
            Background = System.Windows.Media.Brushes.Transparent
        };

        private static readonly List<string> Metricas = new()
        {
            "Peso máximo",
            "Volumen total"
        };

        public ProgresionView()
        {
            InitializeComponent();
            ChartContainer.Child = _grafica;   // añadir la gráfica al contenedor XAML
            InicializarSelectores();
            ActualizarMetricasGenerales();
        }

        // ──────────────────────────────────────────────────────────────────
        // Inicialización
        // ──────────────────────────────────────────────────────────────────

        private void InicializarSelectores()
        {
            var ejercicios = _ejercicioDao.GetAll() ?? new List<Ejercicio>();
            CmbEjercicio.ItemsSource = ejercicios;

            CmbMetrica.ItemsSource   = Metricas;
            CmbMetrica.SelectedIndex = 0;

            if (ejercicios.Any())
                CmbEjercicio.SelectedIndex = 0;
        }

        // ──────────────────────────────────────────────────────────────────
        // Tarjetas de métricas
        // ──────────────────────────────────────────────────────────────────

        private void ActualizarMetricasGenerales()
        {
            TxtTotalSesiones.Text = ObtenerTotalSesiones().ToString();
        }

        private void ActualizarMetricasEjercicio(int ejercicioId)
        {
            double pesoMax = ObtenerPesoMaximo(ejercicioId);
            double volUlt  = ObtenerVolumenUltimaSesion(ejercicioId);

            TxtPesoMaximo.Text    = pesoMax > 0 ? $"{pesoMax:F1}" : "—";
            TxtVolumenUltimo.Text = volUlt  > 0 ? $"{volUlt:F0}"  : "—";
        }

        // ──────────────────────────────────────────────────────────────────
        // Consultas de métricas (SQL directo)
        // ──────────────────────────────────────────────────────────────────

        private int ObtenerTotalSesiones()
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand("SELECT COUNT(*) FROM Sesion", conn);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        private double ObtenerPesoMaximo(int ejercicioId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(
                    "SELECT COALESCE(MAX(PesoKg), 0) FROM SerieRegistro WHERE EjercicioId = @Id",
                    conn);
                cmd.Parameters.AddWithValue("@Id", ejercicioId);
                return Convert.ToDouble(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        private double ObtenerVolumenUltimaSesion(int ejercicioId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(@"
                    SELECT COALESCE(SUM(sr.Repeticiones * sr.PesoKg), 0)
                    FROM SerieRegistro sr
                    WHERE sr.SesionId = (
                        SELECT s.Id FROM Sesion s
                        JOIN SerieRegistro sr2 ON sr2.SesionId = s.Id
                        WHERE sr2.EjercicioId = @Id
                        ORDER BY s.Fecha DESC LIMIT 1
                    ) AND sr.EjercicioId = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", ejercicioId);
                return Convert.ToDouble(cmd.ExecuteScalar());
            }
            catch { return 0; }
        }

        // ──────────────────────────────────────────────────────────────────
        // Gráfica
        // ──────────────────────────────────────────────────────────────────

        private void ActualizarGrafica()
        {
            if (CmbEjercicio.SelectedItem is not Ejercicio ejercicio) return;
            if (CmbMetrica.SelectedItem is not string metrica) return;

            List<EstadisticasService.DataPoint> puntos = metrica == "Peso máximo"
                ? _estadisticasService.GetProgresionPesoMaximo(ejercicio.Id)
                : _estadisticasService.GetVolumenTotal(ejercicio.Id);

            var valores = puntos?.Select(p => p.Valor).ToArray() ?? Array.Empty<double>();
            var labels  = puntos?.Select(p => p.Fecha.ToString("dd/MM/yy")).ToArray() ?? Array.Empty<string>();

            // Usar tipos concretos para evitar referencias directas a ISeries/ICartesianAxis
            _grafica.Series = new LineSeries<double>[]
            {
                new LineSeries<double>
                {
                    Values           = valores,
                    Name             = metrica,
                    Stroke           = new SolidColorPaint(SKColor.Parse("#1D9E75")) { StrokeThickness = 3 },
                    GeometryStroke   = new SolidColorPaint(SKColor.Parse("#1D9E75")) { StrokeThickness = 3 },
                    GeometryFill     = new SolidColorPaint(SKColor.Parse("#1D9E75")),
                    Fill             = new LinearGradientPaint(
                                           SKColor.Parse("#331D9E75"),
                                           SKColor.Parse("#001D9E75")),
                    GeometrySize     = 8,
                    LineSmoothness   = 0.5
                }
            };

            _grafica.XAxes = new[]
            {
                new Axis
                {
                    Labels      = labels,
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize    = 11
                }
            };

            _grafica.YAxes = new[]
            {
                new Axis
                {
                    LabelsPaint = new SolidColorPaint(SKColor.Parse("#AAAAAA")),
                    TextSize    = 11
                }
            };
        }

        // ──────────────────────────────────────────────────────────────────
        // Eventos
        // ──────────────────────────────────────────────────────────────────

        private void CmbEjercicio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbEjercicio.SelectedItem is Ejercicio ejercicio)
                ActualizarMetricasEjercicio(ejercicio.Id);
            ActualizarGrafica();
        }

        private void CmbMetrica_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ActualizarGrafica();
    }
}
