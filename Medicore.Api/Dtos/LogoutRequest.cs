using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class LogoutRequest
{
    [StringLength(200)]
    public string? RefreshToken { get; set; }
}
