namespace Medicore.Api.Models;

public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
