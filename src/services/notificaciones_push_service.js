import { Capacitor } from '@capacitor/core';
import { PushNotifications } from '@capacitor/push-notifications';
import { ajax_post } from '@/ajax_services/ajax_service';
import { sesion_store } from '@/stores/sesion_store';
import { notificaciones_push_store } from '@/stores/notificaciones_push_store';

let listenersInstalados;
let esperaRegistro;
const CLAVE_PUSH_ACTIVADO = 'medicore_push_activado';

export async function registrar_dispositivo_push_service() {
  if (!Capacitor.isNativePlatform() || Capacitor.getPlatform() !== 'android' ||
      !Capacitor.isPluginAvailable('PushNotifications')) {
    return guardar_resultado({
      ok: false,
      mensaje: 'Las notificaciones push requieren la aplicación Android con Firebase configurado.'
    });
  }

  try {
    await instalar_listeners();
    let permisos = await PushNotifications.checkPermissions();
    if (permisos.receive !== 'granted') {
      permisos = await PushNotifications.requestPermissions();
    }
    if (permisos.receive !== 'granted') {
      return {
        ok: false,
        mensaje: 'No se concedió el permiso de notificaciones push. MediCore seguirá funcionando.'
      };
    }

    const token = await solicitar_token_y_esperar();
    localStorage.setItem(CLAVE_PUSH_ACTIVADO, 'true');
    return guardar_resultado({
      ok: true,
      mensaje: 'El dispositivo quedó registrado para notificaciones push.',
      token
    });
  } catch (error) {
    return guardar_resultado({
      ok: false,
      mensaje: error instanceof Error
        ? `No se pudo registrar el dispositivo push: ${error.message}`
        : 'No se pudo registrar el dispositivo push. Verificá la configuración de Firebase.'
    });
  }
}

export async function restaurar_registro_push_service() {
  let pushActivado = false;
  try {
    pushActivado = localStorage.getItem(CLAVE_PUSH_ACTIVADO) === 'true';
  } catch (error) {
    return guardar_resultado({
      ok: false,
      mensaje: error instanceof Error
        ? `No se pudo consultar la preferencia push: ${error.message}`
        : 'No se pudo consultar la preferencia push.'
    });
  }
  if (!pushActivado || !sesion_store.autenticado) {
    return { ok: true, mensaje: '' };
  }

  if (!Capacitor.isNativePlatform() || Capacitor.getPlatform() !== 'android' ||
      !Capacitor.isPluginAvailable('PushNotifications')) {
    return guardar_resultado({
      ok: false,
      mensaje: 'El registro push está guardado, pero solo se puede restaurar en Android.'
    });
  }

  try {
    await instalar_listeners();
    const permisos = await PushNotifications.checkPermissions();
    if (permisos.receive !== 'granted') {
      return guardar_resultado({
        ok: false,
        mensaje: 'El permiso push ya no está concedido. Podés volver a activarlo desde Mi cuenta.'
      });
    }
    const token = await solicitar_token_y_esperar();
    return guardar_resultado({
      ok: true,
      mensaje: 'Se actualizó el registro push del dispositivo.',
      token
    });
  } catch (error) {
    return guardar_resultado({
      ok: false,
      mensaje: error instanceof Error
        ? `No se pudo restaurar el registro push: ${error.message}`
        : 'No se pudo restaurar el registro push.'
    });
  }
}

function guardar_resultado(resultado) {
  notificaciones_push_store.mensaje = resultado.mensaje;
  if (resultado.token) notificaciones_push_store.token = resultado.token;
  return resultado;
}

async function solicitar_token_y_esperar() {
  const tokenRegistrado = new Promise((resolve, reject) => {
    const timer = window.setTimeout(() => {
      esperaRegistro = null;
      reject(new Error('Firebase no devolvió un token en el tiempo esperado.'));
    }, 20000);
    esperaRegistro = {
      resolve: () => {
        window.clearTimeout(timer);
        esperaRegistro = null;
        resolve();
      },
      reject: error => {
        window.clearTimeout(timer);
        esperaRegistro = null;
        reject(error);
      }
    };
  });

  try {
    await PushNotifications.register();
  } catch (error) {
    esperaRegistro?.reject(error);
  }
  await tokenRegistrado;
}

async function instalar_listeners() {
  if (!listenersInstalados) {
    listenersInstalados = (async () => {
      await PushNotifications.addListener('registration', async registro => {
        try {
          if (!sesion_store.autenticado) {
            throw new Error('Iniciá sesión para registrar este dispositivo.');
          }
          await ajax_post('/push/dispositivos', {
            token: registro.value,
            plataforma: Capacitor.getPlatform()
          });
          if (!esperaRegistro) {
            notificaciones_push_store.token = registro.value;
            notificaciones_push_store.mensaje = 'Se actualizó el token push del dispositivo.';
          }
          esperaRegistro?.resolve(registro.value);
        } catch (error) {
          if (esperaRegistro) {
            esperaRegistro.reject(error);
          } else {
            guardar_resultado({
              ok: false,
              mensaje: error instanceof Error ? error.message : 'No se pudo actualizar el token push.'
            });
            window.dispatchEvent(new CustomEvent('medicore:push-error', {
              detail: { mensaje: error instanceof Error ? error.message : 'No se pudo actualizar el token push.' }
            }));
          }
        }
      });
      await PushNotifications.addListener('registrationError', error => {
        const mensaje = error.error || 'Firebase no pudo registrar el dispositivo.';
        if (esperaRegistro) {
          esperaRegistro.reject(new Error(mensaje));
        } else {
          guardar_resultado({ ok: false, mensaje });
          window.dispatchEvent(new CustomEvent('medicore:push-error', { detail: { mensaje } }));
        }
      });
      await PushNotifications.addListener('pushNotificationReceived', () => {
        window.dispatchEvent(new CustomEvent('medicore:push-recibido'));
      });
    })();
  }
  await listenersInstalados;
}
