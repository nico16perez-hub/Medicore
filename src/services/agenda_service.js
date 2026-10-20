import { ajax_get, ajax_put } from '@/ajax_services/ajax_service';

export function obtener_doctores_agenda_service() {
  return ajax_get('/agenda/doctores');
}

export function obtener_doctores_directorio_service() {
  return ajax_get('/directorio/doctores');
}

export function obtener_especialidades_doctor_service(doctor_usuario_id) {
  return ajax_get(`/agenda/doctores/${doctor_usuario_id}/especialidades`);
}

export function guardar_especialidades_doctor_service(doctor_usuario_id, especialidades) {
  return ajax_put(`/agenda/doctores/${doctor_usuario_id}/especialidades`, especialidades);
}

export function obtener_pacientes_agenda_service() {
  return ajax_get('/agenda/pacientes');
}

export function obtener_disponibilidad_doctor_service(doctor_usuario_id) {
  return ajax_get(`/agenda/doctores/${doctor_usuario_id}/disponibilidad`);
}

export function guardar_disponibilidad_doctor_service(doctor_usuario_id, disponibilidad) {
  return ajax_put(`/agenda/doctores/${doctor_usuario_id}/disponibilidad`, disponibilidad);
}

export function obtener_horarios_disponibles_service(doctor_usuario_id, desde, dias = 28) {
  const params = new URLSearchParams({
    doctorUsuarioId: String(doctor_usuario_id),
    desde,
    dias: String(dias)
  });
  return ajax_get(`/agenda/disponibles?${params.toString()}`);
}
