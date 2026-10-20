namespace Medicore.Api.Models;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime CreadaEnUtc { get; set; }
    public bool Leida { get; set; }
    public int? TurnoId { get; set; }
    public Turno? Turno { get; set; }
}
