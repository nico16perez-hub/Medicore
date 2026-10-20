import { ajax_delete, ajax_get, ajax_post, ajax_put } from '@/ajax_services/ajax_service';

export async function obtener_turnos_service(filtros = {}) {
  const {
    pagina = 1,
    tamano = 5,
    busqueda = '',
    estado = 'Todos'
  } = filtros;

  const params = new URLSearchParams({
    pagina: String(pagina),
    tamano: String(tamano),
    busqueda,
    estado
  });

  return ajax_get(`/turnos?${params.toString()}`);
}

export function obtener_turno_service(id) {
  return ajax_get(`/turnos/${id}`);
}

export function crear_turno_service(turno) {
  return ajax_post('/turnos', turno);
}

export function modificar_turno_service(id, turno) {
  return ajax_put(`/turnos/${id}`, turno);
}

export function eliminar_turno_service(id) {
  return ajax_delete(`/turnos/${id}`);
}
