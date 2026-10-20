using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public sealed record UbicacionRequest(
    [property: Range(typeof(decimal), "-90", "90")] decimal Latitud,
    [property: Range(typeof(decimal), "-180", "180")] decimal Longitud);
