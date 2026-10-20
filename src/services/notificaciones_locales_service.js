import { Capacitor } from '@capacitor/core';
import { LocalNotifications } from '@capacitor/local-notifications';

export async function notificar_solicitud_turno_local_service(turnoId) {
  if (!Capacitor.isNativePlatform() ||
      !Capacitor.isPluginAvailable('LocalNotifications')) {
    return {
      ok: false,
      mensaje: 'Las notificaciones locales solo están disponibles en la aplicación móvil.'
    };
  }

  const notificacionId = Number(turnoId);
  if (!Number.isSafeInteger(notificacionId) || notificacionId < 1) {
    return { ok: false, mensaje: 'No se pudo identificar el turno para programar el aviso local.' };
  }

  try {
    let permisos = await LocalNotifications.checkPermissions();
    if (permisos.display !== 'granted') {
      permisos = await LocalNotifications.requestPermissions();
    }
    if (permisos.display !== 'granted') {
      return {
        ok: false,
        mensaje: 'No se concedió el permiso de notificaciones. El turno quedó registrado igualmente.'
      };
    }

    const pendientes = await LocalNotifications.getPending();
    if (pendientes.notifications.some(notificacion => notificacion.id === notificacionId)) {
      return { ok: true, mensaje: 'El aviso de este turno ya estaba programado.' };
    }

    await LocalNotifications.schedule({
      notifications: [{
        id: notificacionId,
        title: 'Solicitud de turno registrada',
        body: 'Tu solicitud quedó guardada en MediCore.',
        schedule: { at: new Date(Date.now() + 1000) }
      }]
    });
    return { ok: true, mensaje: 'Se programó el aviso local de confirmación.' };
  } catch (error) {
    return {
      ok: false,
      mensaje: error instanceof Error
        ? `El turno quedó registrado, pero no se pudo programar el aviso: ${error.message}`
        : 'El turno quedó registrado, pero no se pudo programar el aviso local.'
    };
  }
}
