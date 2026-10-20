import { ajax_get, ajax_put } from '@/ajax_services/ajax_service';

export function obtener_notificaciones_service(pagina = 1, tamano = 20) {
  const params = new URLSearchParams({ pagina: String(pagina), tamano: String(tamano) });
  return ajax_get(`/notificaciones?${params.toString()}`);
}

export function obtener_no_leidas_service() {
  return ajax_get('/notificaciones/no-leidas');
}

export function marcar_notificacion_leida_service(id) {
  return ajax_put(`/notificaciones/${id}/leida`, {});
}

export function marcar_todas_leidas_service() {
  return ajax_put('/notificaciones/leidas', {});
}
