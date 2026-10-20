import { Capacitor } from '@capacitor/core';
import { Geolocation } from '@capacitor/geolocation';
import { Browser } from '@capacitor/browser';
import { ajax_delete, ajax_get, ajax_put } from '@/ajax_services/ajax_service';

export function obtener_ubicacion_perfil_service() {
  return ajax_get('/perfil/ubicacion');
}

export function guardar_ubicacion_perfil_service(ubicacion) {
  return ajax_put('/perfil/ubicacion', ubicacion);
}

export function eliminar_ubicacion_perfil_service() {
  return ajax_delete('/perfil/ubicacion');
}

export async function obtener_ubicacion_actual_service() {
  try {
    let permisos = await Geolocation.checkPermissions();
    if (!tiene_permiso_ubicacion(permisos)) {
      permisos = await Geolocation.requestPermissions();
    }
    if (!tiene_permiso_ubicacion(permisos)) {
      return {
        ok: false,
        ubicacion: null,
        mensaje: 'No se concedió el permiso de ubicación. Podés seguir usando MediCore sin registrarla.'
      };
    }

    const posicion = await Geolocation.getCurrentPosition({
      enableHighAccuracy: true,
      timeout: 15000,
      maximumAge: 0
    });
    return {
      ok: true,
      ubicacion: {
        latitud: posicion.coords.latitude,
        longitud: posicion.coords.longitude
      },
      mensaje: 'Ubicación obtenida.'
    };
  } catch (error) {
    return {
      ok: false,
      ubicacion: null,
      mensaje: mensaje_error_ubicacion(error)
    };
  }
}

export async function abrir_mapa_service({ latitud, longitud, direccion }, modo = 'ver') {
  let query;
  if (Number.isFinite(Number(latitud)) && Number.isFinite(Number(longitud)) &&
      latitud !== null && longitud !== null) {
    query = `${latitud},${longitud}`;
  } else if (typeof direccion === 'string' && direccion.trim()) {
    query = direccion.trim();
  } else {
    return { ok: false, mensaje: 'No hay coordenadas ni una dirección para abrir en el mapa.' };
  }

  const parametro = modo === 'llegar' ? 'destination' : 'query';
  const ruta = modo === 'llegar' ? 'dir' : 'search';
  const url = `https://www.google.com/maps/${ruta}/?api=1&${parametro}=${encodeURIComponent(query)}`;

  try {
    if (Capacitor.isNativePlatform()) {
      if (!Capacitor.isPluginAvailable('Browser')) {
        return { ok: false, mensaje: 'No está disponible la apertura del mapa en este dispositivo.' };
      }
      await Browser.open({ url });
      return { ok: true, mensaje: 'Se abrió el mapa.' };
    }

    const ventana = window.open(url, '_blank');
    if (!ventana) {
      return { ok: false, mensaje: 'El navegador bloqueó la apertura del mapa.' };
    }
    ventana.opener = null;
    return { ok: true, mensaje: 'Se abrió el mapa.' };
  } catch (error) {
    return {
      ok: false,
      mensaje: error instanceof Error ? error.message : 'No se pudo abrir el mapa.'
    };
  }
}

function mensaje_error_ubicacion(error) {
  const codigo = error && typeof error === 'object' ? error.code : undefined;
  if (codigo === 1 || codigo === 'PERMISSION_DENIED') {
    return 'No se concedió el permiso de ubicación. Podés seguir usando MediCore sin registrarla.';
  }
  if (codigo === 2 || codigo === 'POSITION_UNAVAILABLE') {
    return 'No se pudo obtener la ubicación. Revisá que el GPS esté activado e intentá nuevamente.';
  }
  if (codigo === 3 || codigo === 'TIMEOUT') {
    return 'La ubicación tardó demasiado. Intentá nuevamente cuando tengas mejor señal.';
  }
  return error instanceof Error
    ? `No se pudo obtener la ubicación: ${error.message}`
    : 'No se pudo obtener la ubicación. Podés continuar sin registrarla.';
}

function tiene_permiso_ubicacion(permisos) {
  return permisos.location === 'granted' || permisos.coarseLocation === 'granted';
}
