namespace Medicore.Api.Models;

public class Turno
{
    public int Id { get; set; }
    public string Profesional { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string Estado { get; set; } = string.Empty;
}
