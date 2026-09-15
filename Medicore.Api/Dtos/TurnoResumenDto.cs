namespace Medicore.Api.Dtos;

public record TurnoResumenDto(
    int Id,
    string Profesional,
    string Especialidad,
    DateOnly Fecha,
    TimeOnly Hora,
    string Estado
);
