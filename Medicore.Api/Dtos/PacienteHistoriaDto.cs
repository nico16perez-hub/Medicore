namespace Medicore.Api.Dtos;

public record PacienteHistoriaDto(
    int UsuarioId,
    string Email,
    DateOnly? ProximoTurno);
