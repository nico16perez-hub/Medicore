namespace Medicore.Api.Dtos;

public record UsuarioSesionDto(
    int Id,
    string Email,
    bool Activo,
    RolSesionDto? Rol);

public record RolSesionDto(
    int Id,
    string Nombre,
    string Codigo);

public record SesionDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiraEn,
    UsuarioSesionDto Usuario);
