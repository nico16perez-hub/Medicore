using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class TurnoRequest
{
    public int? PacienteUsuarioId { get; set; }
    public int? DoctorUsuarioId { get; set; }
    public int? DoctorPreferidoUsuarioId { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string Profesional { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(120)]
    public string Especialidad { get; set; } = string.Empty;

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(40)]
    public string Estado { get; set; } = string.Empty;
}
