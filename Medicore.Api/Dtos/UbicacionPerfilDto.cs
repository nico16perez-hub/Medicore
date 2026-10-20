namespace Medicore.Api.Dtos;

public sealed record UbicacionPerfilDto(
    decimal Latitud,
    decimal Longitud,
    DateTime ActualizadaEnUtc);
