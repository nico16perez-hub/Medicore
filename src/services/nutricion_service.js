import { ajax_get } from '@/ajax_services/ajax_service';

export async function obtener_nutricion_service() {
  const respuesta = await ajax_get('/nutricion');
  return respuesta.plan || [];
}
