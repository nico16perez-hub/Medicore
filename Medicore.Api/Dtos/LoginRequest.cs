using System.ComponentModel.DataAnnotations;

namespace Medicore.Api.Dtos;

public class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Password { get; set; } = string.Empty;
}
