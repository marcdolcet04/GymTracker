using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GymTracker.Data;
using GymTracker.Data.DAO;
using GymTracker.Models;
using Microsoft.Data.Sqlite;

namespace GymTracker.Views
{
    public partial class EjerciciosView : UserControl
    {
        private readonly EjercicioDAO _dao = new EjercicioDAO();

        private List<Ejercicio> _todos = new();   // lista completa cargada de la BD
        private Ejercicio _editando = null;        // null = modo nuevo, !null = modo edición

        private static readonly List<string> GruposMusculares = new()
        {
            "Todos",
            "Pecho", "Espalda", "Hombros", "Bíceps", "Tríceps",
            "Antebrazos", "Core", "Cuádriceps", "Isquiotibiales",
            "Glúteos", "Gemelos", "Cardio", "Otro"
        };

        public EjerciciosView()
        {
            InitializeComponent();
            InicializarComboBoxes();
            CargarEjercicios();
        }

        // ──────────────────────────────────────────────────────────────────
        // Inicialización
        // ──────────────────────────────────────────────────────────────────

        private void InicializarComboBoxes()
        {
            // Filtro (incluye "Todos")
            CmbFiltroGrupo.ItemsSource    = GruposMusculares;
            CmbFiltroGrupo.SelectedIndex  = 0;

            // Formulario (sin "Todos")
            CmbGrupoMuscular.ItemsSource  = GruposMusculares.Skip(1).ToList();
            CmbGrupoMuscular.SelectedIndex = 0;
        }

        private void CargarEjercicios()
        {
            _todos = _dao.GetAll() ?? new List<Ejercicio>();
            AplicarFiltros();
        }

        // ──────────────────────────────────────────────────────────────────
        // Filtrado
        // ──────────────────────────────────────────────────────────────────

        private void AplicarFiltros()
        {
            var texto = TxtBuscar.Text.Trim().ToLower();
            var grupo = CmbFiltroGrupo.SelectedItem?.ToString();

            var resultado = _todos.AsEnumerable();

            if (!string.IsNullOrEmpty(texto))
                resultado = resultado.Where(e => e.Nombre.ToLower().Contains(texto));

            if (grupo != null && grupo != "Todos")
                resultado = resultado.Where(e => e.GrupoMuscular == grupo);

            GridEjercicios.ItemsSource = resultado.ToList();
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
            => AplicarFiltros();

        private void CmbFiltroGrupo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => AplicarFiltros();

        // ──────────────────────────────────────────────────────────────────
        // Abrir / cerrar panel de formulario
        // ──────────────────────────────────────────────────────────────────

        private void AbrirFormulario(Ejercicio ejercicio = null)
        {
            _editando = ejercicio;

            if (ejercicio == null)
            {
                FormTitulo.Text        = "Nuevo Ejercicio";
                TxtNombre.Text         = string.Empty;
                TxtDescripcion.Text    = string.Empty;
                CmbGrupoMuscular.SelectedIndex = 0;
            }
            else
            {
                FormTitulo.Text        = "Editar Ejercicio";
                TxtNombre.Text         = ejercicio.Nombre;
                TxtDescripcion.Text    = ejercicio.Descripcion;
                CmbGrupoMuscular.SelectedItem = ejercicio.GrupoMuscular;
            }

            FormColumn.Width = new GridLength(300);
        }

        private void CerrarFormulario()
        {
            _editando      = null;
            FormColumn.Width = new GridLength(0);
        }

        // ──────────────────────────────────────────────────────────────────
        // Eventos de botones
        // ──────────────────────────────────────────────────────────────────

        private void BtnNuevoEjercicio_Click(object sender, RoutedEventArgs e)
            => AbrirFormulario();

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Ejercicio ejercicio)
                AbrirFormulario(ejercicio);
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not Ejercicio ejercicio) return;

            bool tieneHistorial = EjercicioTieneSeriesRegistradas(ejercicio.Id);

            string mensaje = $"¿Estás seguro de que quieres eliminar el ejercicio \"{ejercicio.Nombre}\"? Esta acción no se puede deshacer.";
            if (tieneHistorial)
                mensaje += "\n\nEste ejercicio tiene series registradas en el historial que también se eliminarán.";

            var resultado = MessageBox.Show(mensaje, "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            if (!_dao.Delete(ejercicio.Id))
            {
                MessageBox.Show("Error al eliminar el ejercicio.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CargarEjercicios();
        }

        private bool EjercicioTieneSeriesRegistradas(int ejercicioId)
        {
            try
            {
                using var conn = DatabaseHelper.GetConnection();
                using var cmd  = new SqliteCommand(
                    "SELECT COUNT(*) FROM SerieRegistro WHERE EjercicioId = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", ejercicioId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch { return false; }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
            => CerrarFormulario();

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // ── Validaciones ──
            var nombre = TxtNombre.Text.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("El nombre del ejercicio no puede estar vacío.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            bool nombreDuplicado = _todos.Any(ex =>
                ex.Nombre.ToLower() == nombre.ToLower() &&
                ex.Id != (_editando?.Id ?? 0));

            if (nombreDuplicado)
            {
                MessageBox.Show($"Ya existe un ejercicio con el nombre \"{nombre}\".",
                    "Nombre duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            // ── Guardar ──
            if (_editando == null)
            {
                // Insertar nuevo
                var nuevo = new Ejercicio
                {
                    Nombre        = nombre,
                    GrupoMuscular = CmbGrupoMuscular.SelectedItem?.ToString(),
                    Descripcion   = TxtDescripcion.Text.Trim()
                };

                bool ok = _dao.Insert(nuevo);
                if (!ok)
                {
                    MessageBox.Show("Error al guardar el ejercicio.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                // Actualizar existente
                _editando.Nombre        = nombre;
                _editando.GrupoMuscular = CmbGrupoMuscular.SelectedItem?.ToString();
                _editando.Descripcion   = TxtDescripcion.Text.Trim();

                bool ok = _dao.Update(_editando);
                if (!ok)
                {
                    MessageBox.Show("Error al actualizar el ejercicio.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            CerrarFormulario();
            CargarEjercicios();
        }
    }
}
