import { ajax_get_blob } from '@/ajax_services/ajax_service';

export function obtener_comprobante_service(turno_id) {
  return ajax_get_blob(`/turnos/${turno_id}/comprobante`);
}
