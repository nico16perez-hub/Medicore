using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class RefreshRequest
{
    [Required, StringLength(200)]
    public string RefreshToken { get; set; } = string.Empty;
}
