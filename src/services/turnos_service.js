import { ajax_get } from '@/ajax_services/ajax_service';

export async function obtener_turnos_service() {
  const respuesta = await ajax_get('/turnos');
  return respuesta.turnos || [];
}
