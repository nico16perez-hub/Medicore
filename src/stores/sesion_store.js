import { reactive } from 'vue';
import { Capacitor } from '@capacitor/core';
import { SecureStorage } from '@aparajita/capacitor-secure-storage';
import { obtener_api_url } from '@/config/debug';

const CLAVE_SESION = 'sesion';
const CLAVE_BIOMETRIA = 'biometria_habilitada';
const ALMACENAMIENTO_NATIVO = Capacitor.isNativePlatform();

/** @type {{ usuario: { id: number, email: string, activo: boolean, rol: { id: number, nombre: string, codigo: string } | null } | null, token: string | null, refreshToken: string | null, autenticado: boolean, restaurando: boolean, restaurada: boolean, nativeSecureStorage: boolean, biometricEnabled: boolean, biometricAvailable: boolean, requiereBiometria: boolean, error: string }} */
export const sesion_store = reactive({
  usuario: null,
  token: null,
  refreshToken: null,
  autenticado: false,
  restaurando: false,
  restaurada: false,
  nativeSecureStorage: ALMACENAMIENTO_NATIVO,
  biometricEnabled: false,
  biometricAvailable: false,
  requiereBiometria: false,
  error: ''
});

let storageReady;
let restoreInProgress;
let refreshInProgress;

async function preparar_almacenamiento() {
  if (!storageReady) {
    storageReady = SecureStorage.setKeyPrefix('medicore_session');
  }
  await storageReady;
}

function url_api(ruta) {
  return `${obtener_api_url()}/${ruta.replace(/^\//, '')}`;
}

async function respuesta_json(respuesta) {
  let cuerpo = {};
  try {
    cuerpo = await respuesta.json();
  } catch {
    cuerpo = {};
  }
  if (!respuesta.ok) {
    throw new Error(cuerpo.mensaje || `La API respondió con estado ${respuesta.status}.`);
  }
  return cuerpo;
}

async function solicitar(ruta, metodo, datos, token) {
  const url = url_api(ruta);
  let respuesta;
  try {
    respuesta = await fetch(url, {
      method: metodo,
      headers: {
        Accept: 'application/json',
        ...(datos ? { 'Content-Type': 'application/json' } : {}),
        ...(token ? { Authorization: `Bearer ${token}` } : {})
      },
      ...(datos ? { body: JSON.stringify(datos) } : {})
    });
  } catch {
    throw new Error('No se pudo conectar con la API. Revisa que esté levantada y que la URL sea correcta.');
  }
  return respuesta;
}

async function guardar_sesion(sesion) {
  if (ALMACENAMIENTO_NATIVO) {
    await preparar_almacenamiento();
  }
  const rolAnterior = sesion_store.usuario?.rol?.codigo || null;
  const estabaAutenticado = sesion_store.autenticado;
  const valor = {
    accessToken: sesion.access_token,
    refreshToken: sesion.refresh_token
  };
  if (ALMACENAMIENTO_NATIVO) {
    await SecureStorage.set(CLAVE_SESION, valor);
  }
  sesion_store.token = valor.accessToken;
  sesion_store.refreshToken = valor.refreshToken;
  sesion_store.usuario = sesion.usuario;
  sesion_store.autenticado = true;
  sesion_store.requiereBiometria = false;
  sesion_store.error = '';
  const rolActual = sesion_store.usuario?.rol?.codigo || null;
  if (estabaAutenticado && rolAnterior !== rolActual && typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('medicore:role-changed'));
  }
}

async function cargar_sesion_almacenada() {
  await preparar_almacenamiento();
  if (!ALMACENAMIENTO_NATIVO) {
    await Promise.all([
      SecureStorage.remove(CLAVE_SESION),
      SecureStorage.remove(CLAVE_BIOMETRIA)
    ]);
    return null;
  }
  const sesion = await SecureStorage.get(CLAVE_SESION);
  return sesion && typeof sesion === 'object' ? sesion : null;
}

async function obtener_usuario_actual() {
  const respuesta = await solicitar('/sesion/yo', 'GET', null, sesion_store.token);
  if (respuesta.status === 401) {
    const error = new Error('La sesión ya no es válida.');
    error.status = 401;
    throw error;
  }
  return respuesta_json(respuesta);
}

export async function login(email, password) {
  const respuesta = await solicitar('/auth/login', 'POST', { email, password });
  const sesion = await respuesta_json(respuesta);
  await guardar_sesion(sesion);
  sesion_store.restaurada = true;
  return sesion_store.usuario;
}

export async function renovar_sesion() {
  if (refreshInProgress) {
    return refreshInProgress;
  }

  refreshInProgress = (async () => {
    if (!sesion_store.refreshToken) {
      throw new Error('No hay una sesión renovable.');
    }
    const respuesta = await solicitar('/auth/refresh', 'POST', {
      refresh_token: sesion_store.refreshToken
    });
    const sesion = await respuesta_json(respuesta);
    await guardar_sesion(sesion);
    return sesion_store.token;
  })();

  try {
    return await refreshInProgress;
  } finally {
    refreshInProgress = null;
  }
}

export async function restaurar_sesion(opciones = {}) {
  if (sesion_store.autenticado) {
    return true;
  }
  if (restoreInProgress) {
    return restoreInProgress;
  }
  if (sesion_store.restaurada && !opciones.forzar) {
    return false;
  }

  restoreInProgress = (async () => {
    sesion_store.restaurando = true;
    sesion_store.error = '';
    try {
      await preparar_almacenamiento();
      sesion_store.biometricEnabled = ALMACENAMIENTO_NATIVO
        ? Boolean(await SecureStorage.get(CLAVE_BIOMETRIA))
        : false;
      const almacenada = await cargar_sesion_almacenada();
      sesion_store.requiereBiometria = Boolean(
        almacenada && sesion_store.biometricEnabled && !opciones.biometria
      );
      if (!almacenada || sesion_store.requiereBiometria) {
        sesion_store.restaurada = true;
        return false;
      }

      sesion_store.token = almacenada.accessToken;
      sesion_store.refreshToken = almacenada.refreshToken;
      sesion_store.autenticado = true;

      try {
        sesion_store.usuario = await obtener_usuario_actual();
      } catch (error) {
        if (error.status !== 401) {
          throw error;
        }
        await renovar_sesion();
        sesion_store.usuario = await obtener_usuario_actual();
      }

      sesion_store.restaurada = true;
      return true;
    } catch (error) {
      await limpiar_sesion();
      sesion_store.error = error instanceof Error ? error.message : 'No se pudo restaurar la sesión.';
      sesion_store.restaurada = true;
      return false;
    } finally {
      sesion_store.restaurando = false;
    }
  })();

  try {
    return await restoreInProgress;
  } finally {
    restoreInProgress = null;
  }
}

export async function desbloquear_con_biometria() {
  if (!ALMACENAMIENTO_NATIVO) {
    throw new Error('El desbloqueo biométrico solo está disponible en la aplicación móvil.');
  }
  const { BiometricAuth } = await import('@aparajita/capacitor-biometric-auth');
  await preparar_almacenamiento();
  const disponibilidad = await BiometricAuth.checkBiometry();
  sesion_store.biometricAvailable = disponibilidad.isAvailable;
  if (!disponibilidad.isAvailable) {
    throw new Error(disponibilidad.reason || 'La biometría no está disponible en este dispositivo.');
  }
  await BiometricAuth.authenticate({
    reason: 'Desbloquear la sesión de MediCore',
    cancelTitle: 'Usar contraseña',
    androidTitle: 'Desbloquear MediCore',
    allowDeviceCredential: false
  });
  return restaurar_sesion({ biometria: true, forzar: true });
}

export async function habilitar_biometria() {
  if (!ALMACENAMIENTO_NATIVO) {
    throw new Error('El desbloqueo biométrico solo está disponible en la aplicación móvil.');
  }
  const { BiometricAuth } = await import('@aparajita/capacitor-biometric-auth');
  const disponibilidad = await BiometricAuth.checkBiometry();
  sesion_store.biometricAvailable = disponibilidad.isAvailable;
  if (!disponibilidad.isAvailable) {
    throw new Error(disponibilidad.reason || 'La biometría no está disponible en este dispositivo.');
  }
  await BiometricAuth.authenticate({
    reason: 'Confirmar la habilitación del desbloqueo biométrico',
    androidTitle: 'Habilitar biometría',
    allowDeviceCredential: false
  });
  await preparar_almacenamiento();
  await SecureStorage.set(CLAVE_BIOMETRIA, true);
  sesion_store.biometricEnabled = true;
}

export async function deshabilitar_biometria() {
  if (!ALMACENAMIENTO_NATIVO) {
    throw new Error('El desbloqueo biométrico solo está disponible en la aplicación móvil.');
  }
  await preparar_almacenamiento();
  await SecureStorage.remove(CLAVE_BIOMETRIA);
  sesion_store.biometricEnabled = false;
}

export async function limpiar_sesion() {
  await preparar_almacenamiento();
  await Promise.all([
    SecureStorage.remove(CLAVE_SESION),
    SecureStorage.remove(CLAVE_BIOMETRIA)
  ]);
  sesion_store.usuario = null;
  sesion_store.token = null;
  sesion_store.refreshToken = null;
  sesion_store.autenticado = false;
  sesion_store.biometricEnabled = false;
  sesion_store.requiereBiometria = false;
  sesion_store.restaurada = true;
}

export async function logout() {
  const token = sesion_store.token;
  const refreshToken = sesion_store.refreshToken;
  let errorLogout;
  if (token && refreshToken) {
    try {
      const respuesta = await solicitar(
        '/auth/logout',
        'POST',
        { refresh_token: refreshToken },
        token
      );
      if (!respuesta.ok && respuesta.status !== 401) {
        await respuesta_json(respuesta);
      }
    } catch (error) {
      errorLogout = error;
    }
  }

  await limpiar_sesion();
  if (errorLogout) {
    throw errorLogout;
  }
}

export function token_actual() {
  return sesion_store.token;
}

export function notificar_sesion_expirada() {
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new CustomEvent('medicore:session-expired'));
  }
}
