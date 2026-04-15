using System.Windows;
using GymTracker.Data;
using GymTracker.Views;

namespace GymTracker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();

            // Cargar vista inicial
            NavegrarA(new EjerciciosView());
            BtnEjercicios.Tag = "Active";
        }

        private void NavegrarA(object vista)
        {
            MainContent.Content = vista;
        }

        private void ResetarBotones()
        {
            BtnEjercicios.Tag  = null;
            BtnRutinas.Tag     = null;
            BtnNuevaSesion.Tag = null;
            BtnHistorial.Tag   = null;
            BtnProgresion.Tag  = null;
        }

        private void BtnEjercicios_Click(object sender, RoutedEventArgs e)
        {
            ResetarBotones();
            BtnEjercicios.Tag = "Active";
            NavegrarA(new EjerciciosView());
        }

        private void BtnRutinas_Click(object sender, RoutedEventArgs e)
        {
            ResetarBotones();
            BtnRutinas.Tag = "Active";
            NavegrarA(new RutinasView());
        }

        private void BtnNuevaSesion_Click(object sender, RoutedEventArgs e)
        {
            ResetarBotones();
            BtnNuevaSesion.Tag = "Active";
            NavegrarA(new SesionView());
        }

        private void BtnHistorial_Click(object sender, RoutedEventArgs e)
        {
            ResetarBotones();
            BtnHistorial.Tag = "Active";
            NavegrarA(new HistorialView());
        }

        private void BtnProgresion_Click(object sender, RoutedEventArgs e)
        {
            ResetarBotones();
            BtnProgresion.Tag = "Active";
            NavegrarA(new ProgresionView());
        }
    }
}
