import { ajax_delete, ajax_get, ajax_post, ajax_put } from '@/ajax_services/ajax_service';

export async function obtener_nutricion_service(filtros = {}) {
  const {
    pagina = 1,
    tamano = 5,
    busqueda = '',
    dia = 'Todos'
  } = filtros;

  const params = new URLSearchParams({
    pagina: String(pagina),
    tamano: String(tamano),
    busqueda,
    dia
  });

  return ajax_get(`/nutricion?${params.toString()}`);
}

export function crear_item_service(item) {
  return ajax_post('/nutricion', item);
}

export function modificar_item_service(id, item) {
  return ajax_put(`/nutricion/${id}`, item);
}

export function eliminar_item_service(id) {
  return ajax_delete(`/nutricion/${id}`);
}
