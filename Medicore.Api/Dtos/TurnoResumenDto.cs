namespace Medicore.Api.Dtos;

public record TurnoResumenDto(
    int Id,
    int? PacienteUsuarioId,
    int? DoctorUsuarioId,
    string Profesional,
    string Especialidad,
    DateOnly Fecha,
    TimeOnly Hora,
    string Estado,
    string? PacienteEmail = null,
    string? DoctorEmail = null,
    int? DoctorPreferidoUsuarioId = null,
    string? DoctorPreferidoEmail = null
);
