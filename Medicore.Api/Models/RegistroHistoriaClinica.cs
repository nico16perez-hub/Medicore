namespace Medicore.Api.Models;

public class RegistroHistoriaClinica
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string Notas { get; set; } = string.Empty;
}
