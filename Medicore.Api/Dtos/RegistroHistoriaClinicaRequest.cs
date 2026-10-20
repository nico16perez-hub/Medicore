using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class RegistroHistoriaClinicaRequest
{
    public int? PacienteUsuarioId { get; set; }
    public int? DoctorUsuarioId { get; set; }

    public DateOnly Fecha { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    public string Tipo { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    public string Valor { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(240)]
    public string Notas { get; set; } = string.Empty;
}
