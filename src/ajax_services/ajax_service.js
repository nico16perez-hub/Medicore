import { obtener_api_url } from '@/config/debug';

const API_BASE_URL = obtener_api_url();

function construir_url(ruta) {
  if (!API_BASE_URL) {
    throw new Error('No se configuro la URL de la API.');
  }

  return `${API_BASE_URL}/${ruta.replace(/^\//, '')}`;
}

export async function ajax_get(ruta) {
  const url = construir_url(ruta);

  let respuesta;
  try {
    respuesta = await fetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json'
      }
    });
  } catch (error) {
    throw new Error('No se pudo conectar con la API. Revisa que este levantada y que la URL sea correcta.');
  }

  if (!respuesta.ok) {
    throw new Error(`La API respondio con estado ${respuesta.status}.`);
  }

  return respuesta.json();
}

export function obtener_api_base_url() {
  return API_BASE_URL;
}
