namespace GymTracker.Models
{
    public class SerieRegistro
    {
        public int Id { get; set; }
        public int SesionId { get; set; }
        public int EjercicioId { get; set; }
        public int NumSerie { get; set; }
        public int Repeticiones { get; set; }
        public double PesoKg { get; set; }
    }
}
