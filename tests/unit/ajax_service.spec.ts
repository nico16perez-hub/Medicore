import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

const estadoAuth = vi.hoisted(() => ({
  token: 'token-anterior',
  renovaciones: 0,
  limpieza: 0
}));

vi.mock('@/stores/sesion_store', () => ({
  token_actual: () => estadoAuth.token,
  renovar_sesion: async () => {
    estadoAuth.renovaciones += 1;
    await new Promise(resolve => setTimeout(resolve, 10));
    estadoAuth.token = 'token-nuevo';
  },
  limpiar_sesion: async () => {
    estadoAuth.limpieza += 1;
    estadoAuth.token = null;
  },
  notificar_sesion_expirada: vi.fn()
}));

import { ajax_get } from '@/ajax_services/ajax_service';

function respuesta(status: number, cuerpo: object) {
  return {
    ok: status >= 200 && status < 300,
    status,
    json: async () => cuerpo
  };
}

describe('ajax_service authentication', () => {
  beforeEach(() => {
    estadoAuth.token = 'token-anterior';
    estadoAuth.renovaciones = 0;
    estadoAuth.limpieza = 0;
    vi.stubEnv('VITE_API_URL', 'http://localhost:5171/api');
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.unstubAllEnvs();
  });

  test('shares one refresh among concurrent 401 responses and retries each request', async () => {
    const fetchMock = vi.fn(async (_url: string, options: RequestInit) => {
      await new Promise(resolve => setTimeout(resolve, 5));
      if ((options.headers as Record<string, string>).Authorization === 'Bearer token-anterior') {
        return respuesta(401, { mensaje: 'Token vencido' });
      }
      return respuesta(200, { ok: true });
    });
    vi.stubGlobal('fetch', fetchMock);

    await Promise.all(Array.from({ length: 10 }, () => ajax_get('/turnos')));

    expect(estadoAuth.renovaciones).toBe(1);
    expect(fetchMock).toHaveBeenCalledTimes(20);
  });

  test('does not refresh or clear the session after a 403', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => respuesta(403, { mensaje: 'Sin permiso' })));

    await expect(ajax_get('/turnos')).rejects.toThrow('Sin permiso');

    expect(estadoAuth.renovaciones).toBe(0);
    expect(estadoAuth.limpieza).toBe(0);
  });
});
