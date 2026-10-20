using Medicore.Api.Controllers;
using Medicore.Api.Data;
using Medicore.Api.Dtos;
using Medicore.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Controllers;

[ApiController]
[Route("api/sesion")]
[Authorize]
public class SesionController : ControllerBase
{
    private readonly MedicoreDbContext context;

    public SesionController(MedicoreDbContext context)
    {
        this.context = context;
    }

    [HttpGet("yo")]
    public async Task<ActionResult<UsuarioSesionDto>> Yo()
    {
        var usuarioId = AuthClaims.ObtenerUsuarioId(User);
        var usuario = await context.Usuarios
            .AsNoTracking()
            .Include(item => item.Rol)
            .SingleOrDefaultAsync(item => item.Id == usuarioId);

        return usuario is null || !usuario.Activo
            ? Unauthorized(new { codigo = "usuario_inactivo", mensaje = "La sesión ya no es válida." })
            : Ok(AuthController.MapearUsuario(usuario));
    }
}
