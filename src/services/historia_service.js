import { ajax_get } from '@/ajax_services/ajax_service';

export async function obtener_historia_service() {
  const respuesta = await ajax_get('/historia-clinica');
  return respuesta.registros || [];
}
