using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymTracker.Data;
using GymTracker.Data.DAO;
using GymTracker.Models;

namespace GymTracker.Views
{
    public partial class RutinasView : UserControl
    {
        private readonly RutinaDAO   _rutinaDao   = new();
        private readonly EjercicioDAO _ejercicioDao = new();

        private List<Rutina>    _todasRutinas   = new();
        private List<Ejercicio> _todosEjercicios = new();
        private Rutina          _editando        = null;
        private Rutina          _rutinaSeleccionada = null;

        private static readonly List<string> DiasSemana = new()
        {
            "Lunes", "Martes", "Miércoles", "Jueves",
            "Viernes", "Sábado", "Domingo"
        };

        // ViewModel local para los ejercicios asignados (join con nombre)
        private class RutinaEjercicioVM
        {
            public int    Id              { get; set; }
            public int    EjercicioId     { get; set; }
            public string NombreEjercicio { get; set; }
            public string DiaSemana       { get; set; }
            public int    Orden           { get; set; }
        }

        public RutinasView()
        {
            InitializeComponent();
            CmbDiaSemana.ItemsSource = DiasSemana;
            CmbDiaSemana.SelectedIndex = 0;
            CargarDatos();
        }

        // ──────────────────────────────────────────────────────────────────
        // Carga de datos
        // ──────────────────────────────────────────────────────────────────

        private void CargarDatos()
        {
            _todasRutinas    = _rutinaDao.GetAll()    ?? new List<Rutina>();
            _todosEjercicios = _ejercicioDao.GetAll() ?? new List<Ejercicio>();
            GridRutinas.ItemsSource          = _todasRutinas;
            CmbEjercicioAsignar.ItemsSource  = _todosEjercicios;
        }

        private void CargarEjerciciosAsignados(int rutinaId)
        {
            var lista = new List<RutinaEjercicioVM>();
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(@"
                    SELECT re.Id, re.EjercicioId, e.Nombre, re.DiaSemana, re.Orden
                    FROM RutinaEjercicio re
                    JOIN Ejercicio e ON re.EjercicioId = e.Id
                    WHERE re.RutinaId = @RutinaId
                    ORDER BY re.DiaSemana, re.Orden", conn);
                cmd.Parameters.AddWithValue("@RutinaId", rutinaId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    lista.Add(new RutinaEjercicioVM
                    {
                        Id              = reader.GetInt32(0),
                        EjercicioId     = reader.GetInt32(1),
                        NombreEjercicio = reader.GetString(2),
                        DiaSemana       = reader.GetString(3),
                        Orden           = reader.GetInt32(4)
                    });
            }
            catch { }
            GridEjerciciosAsignados.ItemsSource = lista;
        }

        // ──────────────────────────────────────────────────────────────────
        // Panel formulario lateral
        // ──────────────────────────────────────────────────────────────────

        private void AbrirFormulario(Rutina rutina = null)
        {
            _editando = rutina;
            if (rutina == null)
            {
                FormTitulo.Text    = "Nueva Rutina";
                TxtNombre.Text     = string.Empty;
                TxtDescripcion.Text = string.Empty;
            }
            else
            {
                FormTitulo.Text    = "Editar Rutina";
                TxtNombre.Text     = rutina.Nombre;
                TxtDescripcion.Text = rutina.Descripcion;
            }
            FormColumn.Width = new GridLength(300);
        }

        private void CerrarFormulario()
        {
            _editando        = null;
            FormColumn.Width = new GridLength(0);
        }

        // ──────────────────────────────────────────────────────────────────
        // Panel asignación de ejercicios
        // ──────────────────────────────────────────────────────────────────

        private void MostrarPanelAsignacion(Rutina rutina)
        {
            _rutinaSeleccionada = rutina;
            TxtTituloAsignacion.Text = $"Ejercicios — {rutina.Nombre}";
            AsignacionRow.Height     = new GridLength(280);

            // Recargar ejercicios disponibles para garantizar que el ComboBox esté poblado
            _todosEjercicios = _ejercicioDao.GetAll() ?? new System.Collections.Generic.List<Ejercicio>();
            CmbEjercicioAsignar.ItemsSource = _todosEjercicios;
            if (_todosEjercicios.Count > 0)
                CmbEjercicioAsignar.SelectedIndex = 0;

            CargarEjerciciosAsignados(rutina.Id);
        }

        private void OcultarPanelAsignacion()
        {
            _rutinaSeleccionada  = null;
            AsignacionRow.Height = new GridLength(0);
        }

        // ──────────────────────────────────────────────────────────────────
        // Operaciones sobre RutinaEjercicio
        // ──────────────────────────────────────────────────────────────────

        private bool AgregarEjercicioARutina(int rutinaId, int ejercicioId, string diaSemana, int orden)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(@"
                    INSERT INTO RutinaEjercicio (RutinaId, EjercicioId, DiaSemana, Orden)
                    VALUES (@RutinaId, @EjercicioId, @DiaSemana, @Orden)", conn);
                cmd.Parameters.AddWithValue("@RutinaId",    rutinaId);
                cmd.Parameters.AddWithValue("@EjercicioId", ejercicioId);
                cmd.Parameters.AddWithValue("@DiaSemana",   diaSemana);
                cmd.Parameters.AddWithValue("@Orden",       orden);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        private bool QuitarEjercicioDeRutina(int rutinaEjercicioId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(
                    "DELETE FROM RutinaEjercicio WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", rutinaEjercicioId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        private int SiguienteOrden(int rutinaId, string diaSemana)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(@"
                    SELECT COALESCE(MAX(Orden), 0) + 1
                    FROM RutinaEjercicio
                    WHERE RutinaId = @RutinaId AND DiaSemana = @DiaSemana", conn);
                cmd.Parameters.AddWithValue("@RutinaId",  rutinaId);
                cmd.Parameters.AddWithValue("@DiaSemana", diaSemana);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch { return 1; }
        }

        // ──────────────────────────────────────────────────────────────────
        // Eventos
        // ──────────────────────────────────────────────────────────────────

        private void BtnNuevaRutina_Click(object sender, RoutedEventArgs e)
            => AbrirFormulario();

        private void BtnEditarRutina_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Rutina rutina)
                AbrirFormulario(rutina);
        }

        private void BtnEliminarRutina_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not Rutina rutina) return;

            var resultado = MessageBox.Show(
                $"¿Estás seguro de que quieres eliminar la rutina \"{rutina.Nombre}\"? Se eliminarán también todos sus ejercicios asignados.",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            if (!_rutinaDao.Delete(rutina.Id))
            {
                MessageBox.Show("Error al eliminar la rutina.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Si el panel de asignación mostraba esta rutina, ocultarlo
            if (_rutinaSeleccionada?.Id == rutina.Id)
                OcultarPanelAsignacion();

            CargarDatos();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => CerrarFormulario();

        private void GridRutinas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridRutinas.SelectedItem is Rutina rutina)
                MostrarPanelAsignacion(rutina);
            else
                OcultarPanelAsignacion();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var nombre = TxtNombre.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("El nombre de la rutina no puede estar vacío.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            bool duplicado = _todasRutinas.Any(r =>
                r.Nombre.ToLower() == nombre.ToLower() &&
                r.Id != (_editando?.Id ?? 0));

            if (duplicado)
            {
                MessageBox.Show($"Ya existe una rutina con el nombre \"{nombre}\".",
                    "Nombre duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            if (_editando == null)
            {
                var nueva = new Rutina
                {
                    Nombre        = nombre,
                    Descripcion   = TxtDescripcion.Text.Trim(),
                    FechaCreacion = DateTime.Now
                };
                if (!_rutinaDao.Insert(nueva))
                {
                    MessageBox.Show("Error al guardar la rutina.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                _editando.Nombre      = nombre;
                _editando.Descripcion = TxtDescripcion.Text.Trim();
                if (!_rutinaDao.Update(_editando))
                {
                    MessageBox.Show("Error al actualizar la rutina.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            CerrarFormulario();
            CargarDatos();
        }

        private void BtnAnadirEjercicio_Click(object sender, RoutedEventArgs e)
        {
            if (_rutinaSeleccionada == null)
            {
                MessageBox.Show("Selecciona una rutina primero.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (CmbEjercicioAsignar.SelectedItem is not Ejercicio ejercicio)
            {
                MessageBox.Show("Selecciona un ejercicio.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string dia   = CmbDiaSemana.SelectedItem?.ToString();
            int    orden = SiguienteOrden(_rutinaSeleccionada.Id, dia);

            if (!AgregarEjercicioARutina(_rutinaSeleccionada.Id, ejercicio.Id, dia, orden))
            {
                MessageBox.Show("Error al añadir el ejercicio.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CargarEjerciciosAsignados(_rutinaSeleccionada.Id);
        }

        private void BtnQuitarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is RutinaEjercicioVM vm)
            {
                if (!QuitarEjercicioDeRutina(vm.Id))
                {
                    MessageBox.Show("Error al quitar el ejercicio.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CargarEjerciciosAsignados(_rutinaSeleccionada.Id);
            }
        }
    }
}
