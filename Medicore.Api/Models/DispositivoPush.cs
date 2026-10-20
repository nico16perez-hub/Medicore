namespace Medicore.Api.Models;

public class DispositivoPush
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public string Plataforma { get; set; } = string.Empty;
    public DateTime ActualizadoEnUtc { get; set; }
}
