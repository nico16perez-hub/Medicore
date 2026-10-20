using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public sealed record DispositivoPushRequest(
    [property: Required, StringLength(4096, MinimumLength = 1)] string Token,
    [property: Required, StringLength(20, MinimumLength = 1)] string Plataforma);
