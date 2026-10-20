const PREFIJO_QR_TURNO = 'medicore://turno/';

export function crear_contenido_qr_turno(id) {
  if (!Number.isSafeInteger(id) || id < 1) {
    throw new RangeError('El identificador del turno no es válido.');
  }
  return `${PREFIJO_QR_TURNO}${id}`;
}

export function leer_id_qr_turno(contenido) {
  if (typeof contenido !== 'string') return null;
  const coincidencia = contenido.match(/^medicore:\/\/turno\/([1-9]\d*)$/i);
  if (!coincidencia) return null;
  const id = Number(coincidencia[1]);
  return Number.isSafeInteger(id) ? id : null;
}
