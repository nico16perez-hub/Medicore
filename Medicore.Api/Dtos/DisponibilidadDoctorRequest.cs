namespace Medicore.Api.Dtos;

public record DisponibilidadDoctorRequest(
    int DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFin);
