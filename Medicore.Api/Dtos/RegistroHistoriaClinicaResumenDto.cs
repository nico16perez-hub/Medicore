namespace Medicore.Api.Dtos;

public record RegistroHistoriaClinicaResumenDto(
    int Id,
    DateOnly Fecha,
    string Tipo,
    string Valor,
    string Notas
);
