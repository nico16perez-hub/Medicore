import { ajax_get_blob, ajax_post_form } from '@/ajax_services/ajax_service';

export function obtener_foto_perfil_service() {
  return ajax_get_blob('/perfil/foto');
}

export function guardar_foto_perfil_service(archivo) {
  const formulario = new FormData();
  formulario.append('foto', archivo, 'foto-perfil');
  return ajax_post_form('/perfil/foto', formulario);
}
