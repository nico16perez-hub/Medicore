import { ajax_delete, ajax_get, ajax_post, ajax_put } from '@/ajax_services/ajax_service';

export async function obtener_historia_service(filtros = {}) {
  const {
    pagina = 1,
    tamano = 5,
    busqueda = '',
    tipo = 'Todos',
    pacienteUsuarioId
  } = filtros;

  const params = new URLSearchParams({
    pagina: String(pagina),
    tamano: String(tamano),
    busqueda,
    tipo
  });
  if (pacienteUsuarioId) {
    params.set('pacienteUsuarioId', String(pacienteUsuarioId));
  }

  return ajax_get(`/historia-clinica?${params.toString()}`);
}

export function obtener_pacientes_historia_service() {
  return ajax_get('/historia-clinica/pacientes');
}

export function crear_registro_service(registro) {
  return ajax_post('/historia-clinica', registro);
}

export function modificar_registro_service(id, registro) {
  return ajax_put(`/historia-clinica/${id}`, registro);
}

export function eliminar_registro_service(id) {
  return ajax_delete(`/historia-clinica/${id}`);
}
