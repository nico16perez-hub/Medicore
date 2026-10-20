import { obtener_api_url } from '@/config/debug';
import {
  limpiar_sesion,
  notificar_sesion_expirada,
  renovar_sesion,
  token_actual
} from '@/stores/sesion_store';

let refreshEnCurso = null;

function construir_url(ruta) {
  return `${obtener_api_url()}/${ruta.replace(/^\//, '')}`;
}

export async function ajax_get(ruta) {
  return ajax_request('GET', ruta);
}

async function ajax_request(metodo, ruta, datos, reintentar = true, tipo_respuesta = 'json') {
  const url = construir_url(ruta);
  const token = token_actual();

  let respuesta;
  try {
    const es_formulario = datos instanceof FormData;
    const opciones = {
      method: metodo,
      headers: {
        Accept: 'application/json',
        ...(datos && !es_formulario ? { 'Content-Type': 'application/json' } : {})
      }
    };
    if (token) {
      opciones.headers.Authorization = `Bearer ${token}`;
    }
    if (datos) {
      opciones.body = es_formulario ? datos : JSON.stringify(datos);
    }
    respuesta = await fetch(url, opciones);
  } catch (error) {
    throw new Error('No se pudo conectar con la API. Revisa que este levantada y que la URL sea correcta.');
  }

  if (respuesta.status === 401 && reintentar && token_actual() &&
      !/\/auth\/(login|refresh|logout)$/.test(ruta)) {
    if (token && token_actual() !== token) {
      return ajax_request(metodo, ruta, datos, false, tipo_respuesta);
    }

    if (!refreshEnCurso) {
      refreshEnCurso = renovar_sesion().finally(() => {
        refreshEnCurso = null;
      });
    }
    try {
      await refreshEnCurso;
    } catch {
      await limpiar_sesion();
      notificar_sesion_expirada();
      throw new Error('La sesión expiró. Iniciá sesión nuevamente.');
    }
    return ajax_request(metodo, ruta, datos, false, tipo_respuesta);
  }

  if (!respuesta.ok) {
    let detalle = '';
    try {
      const cuerpo = await respuesta.json();
      detalle = cuerpo.mensaje || '';
    } catch {
      detalle = '';
    }
    throw new Error(detalle || `La API respondio con estado ${respuesta.status}.`);
  }

  if (respuesta.status === 204) return null;
  return tipo_respuesta === 'blob' ? respuesta.blob() : respuesta.json();
}

export function ajax_post(ruta, datos) {
  return ajax_request('POST', ruta, datos);
}

export function ajax_put(ruta, datos) {
  return ajax_request('PUT', ruta, datos);
}

export function ajax_delete(ruta) {
  return ajax_request('DELETE', ruta);
}

export function ajax_post_form(ruta, datos) {
  return ajax_request('POST', ruta, datos);
}

export function ajax_get_blob(ruta) {
  return ajax_request('GET', ruta, undefined, true, 'blob');
}

export function obtener_api_base_url() {
  return obtener_api_url();
}
