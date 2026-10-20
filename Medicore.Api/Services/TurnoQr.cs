namespace Medicore.Api.Services;

public static class TurnoQr
{
    public static string CrearContenido(int turnoId) => $"medicore://turno/{turnoId}";
}
