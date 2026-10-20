namespace Medicore.Api.Dtos;

public sealed record NotificacionDto(
    int Id,
    string Titulo,
    string Mensaje,
    DateTime CreadaEnUtc,
    bool Leida,
    int? TurnoId);
