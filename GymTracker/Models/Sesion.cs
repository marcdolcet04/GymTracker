namespace GymTracker.Models
{
    public class Sesion
    {
        public int Id { get; set; }
        public int RutinaId { get; set; }
        public DateTime Fecha { get; set; }
        public int DuracionMinutos { get; set; }
        public string Notas { get; set; }
    }
}
