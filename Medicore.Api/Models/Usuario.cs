namespace Medicore.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public int? RolId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string? FotoPerfilNombre { get; set; }
    public decimal? UbicacionLatitud { get; set; }
    public decimal? UbicacionLongitud { get; set; }
    public DateTime? UbicacionActualizadaEnUtc { get; set; }
    public Rol? Rol { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
