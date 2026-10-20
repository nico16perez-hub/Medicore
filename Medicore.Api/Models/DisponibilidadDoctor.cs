namespace Medicore.Api.Models;

public class DisponibilidadDoctor
{
    public int Id { get; set; }
    public int DoctorUsuarioId { get; set; }
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
}
