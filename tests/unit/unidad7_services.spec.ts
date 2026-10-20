import { afterEach, describe, expect, test, vi } from 'vitest';

const geolocation = vi.hoisted(() => ({
  checkPermissions: vi.fn(),
  requestPermissions: vi.fn(),
  getCurrentPosition: vi.fn()
}));
const localNotifications = vi.hoisted(() => ({
  checkPermissions: vi.fn(),
  requestPermissions: vi.fn(),
  getPending: vi.fn(),
  schedule: vi.fn()
}));
const capacitor = vi.hoisted(() => ({
  isNativePlatform: vi.fn(),
  isPluginAvailable: vi.fn(),
  getPlatform: vi.fn()
}));

vi.mock('@capacitor/geolocation', () => ({ Geolocation: geolocation }));
vi.mock('@capacitor/local-notifications', () => ({ LocalNotifications: localNotifications }));
vi.mock('@capacitor/core', () => ({ Capacitor: capacitor }));

import {
  abrir_mapa_service,
  obtener_ubicacion_actual_service
} from '@/services/ubicacion_service';
import { notificar_solicitud_turno_local_service } from '@/services/notificaciones_locales_service';

describe('servicios de ubicación y notificaciones locales', () => {
  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  test('permite continuar si se deniega el permiso de ubicación', async () => {
    geolocation.checkPermissions.mockResolvedValue({ location: 'prompt' });
    geolocation.requestPermissions.mockResolvedValue({ location: 'denied' });

    await expect(obtener_ubicacion_actual_service()).resolves.toEqual({
      ok: false,
      ubicacion: null,
      mensaje: expect.stringContaining('No se concedió el permiso')
    });
    expect(geolocation.getCurrentPosition).not.toHaveBeenCalled();
  });

  test('obtiene coordenadas con precisión alta después de conceder el permiso', async () => {
    geolocation.checkPermissions.mockResolvedValue({ location: 'granted' });
    geolocation.getCurrentPosition.mockResolvedValue({
      coords: { latitude: -34.6, longitude: -58.4 }
    });

    await expect(obtener_ubicacion_actual_service()).resolves.toMatchObject({
      ok: true,
      ubicacion: { latitud: -34.6, longitud: -58.4 }
    });
    expect(geolocation.getCurrentPosition).toHaveBeenCalledWith({
      enableHighAccuracy: true,
      timeout: 15000,
      maximumAge: 0
    });
  });

  test('codifica una dirección antes de abrir el mapa', async () => {
    capacitor.isNativePlatform.mockReturnValue(false);
    vi.spyOn(window, 'open').mockReturnValue({} as Window);

    const resultado = await abrir_mapa_service(
      { direccion: 'Av. San Martín 123, Buenos Aires' },
      'llegar'
    );

    expect(resultado.ok).toBe(true);
    expect(window.open).toHaveBeenCalledWith(
      expect.stringContaining('destination=Av.%20San%20Mart%C3%ADn%20123%2C%20Buenos%20Aires'),
      '_blank'
    );
  });

  test('no programa una notificación si se deniega el permiso local', async () => {
    capacitor.isNativePlatform.mockReturnValue(true);
    capacitor.isPluginAvailable.mockReturnValue(true);
    localNotifications.checkPermissions.mockResolvedValue({ display: 'prompt' });
    localNotifications.requestPermissions.mockResolvedValue({ display: 'denied' });

    const resultado = await notificar_solicitud_turno_local_service(25);

    expect(resultado.ok).toBe(false);
    expect(resultado.mensaje).toContain('El turno quedó registrado igualmente');
    expect(localNotifications.schedule).not.toHaveBeenCalled();
  });
});
