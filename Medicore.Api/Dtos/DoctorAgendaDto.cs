namespace Medicore.Api.Dtos;

public record DoctorAgendaDto(
    int Id,
    string Email,
    IReadOnlyList<string>? Especialidades = null);
