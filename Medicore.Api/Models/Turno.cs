namespace Medicore.Api.Models;

public static class TurnoEstados
{
    public const string Pendiente = "Pendiente";
    public const string Confirmado = "Confirmado";
    public const string Reprogramado = "Reprogramado";
    public const string Cancelado = "Cancelado";

    public static readonly string[] Todos =
    {
        Pendiente,
        Confirmado,
        Reprogramado,
        Cancelado
    };
}

public class Turno
{
    public int Id { get; set; }
    public int? PacienteUsuarioId { get; set; }
    public int? DoctorUsuarioId { get; set; }
    public int? DoctorPreferidoUsuarioId { get; set; }
    public string Profesional { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public TimeOnly Hora { get; set; }
    public string Estado { get; set; } = string.Empty;
}
