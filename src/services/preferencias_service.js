import { Capacitor } from '@capacitor/core';
import { Haptics, ImpactStyle, NotificationType } from '@capacitor/haptics';

const CLAVE_VIBRACION = 'medicore_vibracion_habilitada';

export function vibracion_habilitada() {
  return localStorage.getItem(CLAVE_VIBRACION) !== 'false';
}

export function guardar_preferencia_vibracion(habilitada) {
  localStorage.setItem(CLAVE_VIBRACION, String(Boolean(habilitada)));
}

export async function respuesta_haptica(tipo = 'confirmacion') {
  if (!vibracion_habilitada() || !Capacitor.isNativePlatform() ||
      !Capacitor.isPluginAvailable('Haptics')) {
    return false;
  }

  try {
    if (tipo === 'error') {
      await Haptics.notification({ type: NotificationType.Error });
    } else if (tipo === 'confirmacion') {
      await Haptics.notification({ type: NotificationType.Success });
    } else {
      await Haptics.impact({ style: ImpactStyle.Light });
    }
    return true;
  } catch (error) {
    console.warn('La respuesta háptica no está disponible en este dispositivo.', error);
    return false;
  }
}
