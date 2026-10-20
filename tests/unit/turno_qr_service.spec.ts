import { describe, expect, test } from 'vitest';
import { crear_contenido_qr_turno, leer_id_qr_turno } from '@/services/turno_qr_service';

describe('identificador QR del turno', () => {
  test('crea un código estable que solo contiene el identificador', () => {
    const contenido = crear_contenido_qr_turno(25);
    expect(contenido).toBe('medicore://turno/25');
    expect(leer_id_qr_turno(contenido)).toBe(25);
  });

  test('rechaza contenido que no sea un identificador de turno MediCore', () => {
    expect(leer_id_qr_turno('https://example.test?paciente=nombre')).toBeNull();
    expect(leer_id_qr_turno('medicore://turno/0')).toBeNull();
    expect(leer_id_qr_turno('medicore://turno/9007199254740992')).toBeNull();
  });

  test('no permite generar códigos con identificadores inválidos', () => {
    expect(() => crear_contenido_qr_turno(0)).toThrow(RangeError);
    expect(() => crear_contenido_qr_turno(1.5)).toThrow(RangeError);
  });
});
