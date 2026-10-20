namespace Medicore.Api.Dtos;

public record RegistroHistoriaClinicaResumenDto(
    int Id,
    int? PacienteUsuarioId,
    int? DoctorUsuarioId,
    DateOnly Fecha,
    string Tipo,
    string Valor,
    string Notas
);
