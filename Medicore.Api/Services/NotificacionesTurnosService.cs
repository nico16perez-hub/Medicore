using Medicore.Api.Data;
using Medicore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Medicore.Api.Services;

public sealed class NotificacionesTurnosService(MedicoreDbContext context)
{
    public async Task AgregarNotificacionesSolicitudAsync(Turno turno, int usuarioActorId, string? rol)
    {
        if (rol == "paciente")
        {
            var secretarias = await context.Usuarios
                .Where(usuario => usuario.Activo && usuario.Rol != null &&
                    usuario.Rol.Activo && usuario.Rol.Codigo == "secretaria" &&
                    usuario.Id != usuarioActorId)
                .Select(usuario => usuario.Id)
                .ToListAsync();

            Agregar(secretarias, usuarioActorId, turno,
                "Nueva solicitud de turno",
                $"Solicitaron una consulta de {turno.Especialidad} con preferencia para el {turno.Fecha:dd/MM/yyyy} a las {turno.Hora:HH\\:mm}.");
            return;
        }

        if (rol == "secretaria" &&
            (string.Equals(turno.Estado, TurnoEstados.Confirmado, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(turno.Estado, TurnoEstados.Reprogramado, StringComparison.OrdinalIgnoreCase)))
        {
            Agregar(
                UsuariosInteresados(turno),
                usuarioActorId,
                turno,
                "Turno asignado",
                $"Se asignó un turno para el {turno.Fecha:dd/MM/yyyy} a las {turno.Hora:HH\\:mm} con {turno.Profesional}.");
            return;
        }

        Agregar(UsuariosInteresados(turno), usuarioActorId, turno,
            "Turno registrado", "Se registró un turno asociado a tu cuenta.");
    }

    public void AgregarNotificacionesCambio(
        Turno turno,
        int usuarioActorId,
        string? estadoAnterior,
        DateOnly fechaAnterior,
        TimeOnly horaAnterior,
        string profesionalAnterior,
        string especialidadAnterior,
        int? pacienteAnterior,
        int? doctorAnterior,
        int? doctorPreferidoAnterior)
    {
        var huboCambios = turno.Estado != estadoAnterior ||
            turno.Fecha != fechaAnterior ||
            turno.Hora != horaAnterior ||
            turno.Profesional != profesionalAnterior ||
            turno.Especialidad != especialidadAnterior ||
            turno.PacienteUsuarioId != pacienteAnterior ||
            turno.DoctorUsuarioId != doctorAnterior ||
            turno.DoctorPreferidoUsuarioId != doctorPreferidoAnterior;

        if (!huboCambios)
        {
            return;
        }

        var (titulo, mensaje) = !string.Equals(
            turno.Estado, estadoAnterior, StringComparison.OrdinalIgnoreCase)
            ? string.Equals(turno.Estado, TurnoEstados.Cancelado, StringComparison.OrdinalIgnoreCase)
                ? ("Turno cancelado", "Un turno asociado a tu cuenta fue cancelado.")
                : string.Equals(turno.Estado, TurnoEstados.Confirmado, StringComparison.OrdinalIgnoreCase)
                    ? ("Turno asignado", $"Se asignó un turno para el {turno.Fecha:dd/MM/yyyy} a las {turno.Hora:HH\\:mm} con {turno.Profesional}.")
                    : string.Equals(turno.Estado, TurnoEstados.Reprogramado, StringComparison.OrdinalIgnoreCase)
                        ? ("Turno reprogramado", "Un turno asociado a tu cuenta fue reprogramado.")
                        : ("Turno actualizado", "Se actualizó un turno asociado a tu cuenta.")
            : ("Turno actualizado", "Se actualizó un turno asociado a tu cuenta.");

        Agregar(UsuariosInteresados(turno).Concat(UsuariosInteresados(
                new Turno { PacienteUsuarioId = pacienteAnterior, DoctorUsuarioId = doctorAnterior })),
            usuarioActorId, turno, titulo, mensaje);
    }

    private static IEnumerable<int> UsuariosInteresados(Turno turno) =>
        new[] { turno.PacienteUsuarioId, turno.DoctorUsuarioId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct();

    private void Agregar(
        IEnumerable<int> usuarios,
        int usuarioActorId,
        Turno turno,
        string titulo,
        string mensaje)
    {
        var destinatarios = usuarios
            .Where(usuarioId => usuarioId != usuarioActorId)
            .Distinct();

        foreach (var destinatario in destinatarios)
        {
            context.Notificaciones.Add(new Notificacion
            {
                UsuarioId = destinatario,
                Turno = turno,
                Titulo = titulo,
                Mensaje = mensaje,
                CreadaEnUtc = DateTime.UtcNow
            });
        }
    }
}
