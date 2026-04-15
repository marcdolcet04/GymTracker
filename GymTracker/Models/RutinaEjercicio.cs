namespace GymTracker.Models
{
    public class RutinaEjercicio
    {
        public int Id { get; set; }
        public int RutinaId { get; set; }
        public int EjercicioId { get; set; }
        public string DiaSemana { get; set; }
        public int Orden { get; set; }
    }
}
