import { afterEach, describe, expect, test } from 'vitest';
import {
  guardar_preferencia_vibracion,
  vibracion_habilitada
} from '@/services/preferencias_service';

describe('preferencia de vibración', () => {
  afterEach(() => localStorage.clear());

  test('está activa de forma predeterminada y persiste el valor elegido', () => {
    expect(vibracion_habilitada()).toBe(true);
    guardar_preferencia_vibracion(false);
    expect(vibracion_habilitada()).toBe(false);
    guardar_preferencia_vibracion(true);
    expect(vibracion_habilitada()).toBe(true);
  });
});
