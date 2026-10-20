import { afterEach, describe, expect, test, vi } from 'vitest';

const secureStorage = vi.hoisted(() => ({
  setKeyPrefix: vi.fn(async () => undefined),
  get: vi.fn(async () => null),
  set: vi.fn(async () => undefined),
  remove: vi.fn(async () => true)
}));

vi.mock('@capacitor/core', () => ({
  Capacitor: { isNativePlatform: () => false }
}));

vi.mock('@aparajita/capacitor-secure-storage', () => ({
  SecureStorage: secureStorage
}));

vi.mock('@/config/debug', () => ({
  obtener_api_url: () => 'http://localhost:5171/api'
}));

import { login, restaurar_sesion, sesion_store } from '@/stores/sesion_store';

describe('sesión web sin persistencia de credenciales', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    secureStorage.setKeyPrefix.mockClear();
    secureStorage.get.mockClear();
    secureStorage.set.mockClear();
    secureStorage.remove.mockClear();
    sesion_store.usuario = null;
    sesion_store.token = null;
    sesion_store.refreshToken = null;
    sesion_store.autenticado = false;
    sesion_store.restaurando = false;
    sesion_store.restaurada = false;
    sesion_store.biometricEnabled = false;
    sesion_store.requiereBiometria = false;
    sesion_store.error = '';
  });

  test('mantiene tokens en memoria y limpia las claves web legadas al restaurar', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => ({
      ok: true,
      status: 200,
      json: async () => ({
        access_token: 'access-en-memoria',
        refresh_token: 'refresh-en-memoria',
        usuario: { id: 1, email: 'paciente@medicore.local', activo: true, rol: null }
      })
    })));

    await login('paciente@medicore.local', 'contraseña');

    expect(sesion_store.token).toBe('access-en-memoria');
    expect(sesion_store.refreshToken).toBe('refresh-en-memoria');
    expect(secureStorage.set).not.toHaveBeenCalled();

    sesion_store.usuario = null;
    sesion_store.token = null;
    sesion_store.refreshToken = null;
    sesion_store.autenticado = false;
    sesion_store.restaurada = false;

    await expect(restaurar_sesion()).resolves.toBe(false);
    expect(secureStorage.remove).toHaveBeenCalledWith('sesion');
    expect(secureStorage.remove).toHaveBeenCalledWith('biometria_habilitada');
  });
});
