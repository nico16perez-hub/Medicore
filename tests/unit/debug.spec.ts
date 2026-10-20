import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

const platform = vi.hoisted(() => ({ name: 'web' }));

vi.mock('@capacitor/core', () => ({
  Capacitor: {
    getPlatform: () => platform.name,
    isNativePlatform: () => platform.name !== 'web'
  }
}));

import { obtener_api_url } from '@/config/debug';

describe('API transport configuration', () => {
  beforeEach(() => {
    platform.name = 'web';
  });

  afterEach(() => {
    vi.unstubAllEnvs();
  });

  test('rejects insecure HTTP URLs for non-loopback hosts', () => {
    vi.stubEnv('VITE_API_URL', 'http://192.168.1.25:5171/api');

    expect(() => obtener_api_url()).toThrow(/HTTP solo se permite para localhost/);
  });

  test('allows HTTP only for the Android emulator with an explicit native-build opt-in', () => {
    platform.name = 'android';
    vi.stubEnv('DEV', false);
    vi.stubEnv('VITE_API_URL', 'http://10.0.2.2:5171/api');
    vi.stubEnv('VITE_ALLOW_INSECURE_LOCAL_API', 'true');

    expect(obtener_api_url()).toBe('http://10.0.2.2:5171/api');
  });

  test('allows an explicitly opted-in private LAN API on Android', () => {
    platform.name = 'android';
    vi.stubEnv('DEV', false);
    vi.stubEnv('VITE_API_URL', 'http://192.168.1.25:5171/api');
    vi.stubEnv('VITE_ALLOW_INSECURE_LOCAL_API', 'true');

    expect(obtener_api_url()).toBe('http://192.168.1.25:5171/api');
  });

  test('does not allow insecure public hosts even with the development opt-in', () => {
    platform.name = 'android';
    vi.stubEnv('DEV', false);
    vi.stubEnv('VITE_API_URL', 'http://example.com:5171/api');
    vi.stubEnv('VITE_ALLOW_INSECURE_LOCAL_API', 'true');

    expect(() => obtener_api_url()).toThrow(/API debe usar HTTPS/);
  });

  test('does not allow the Android emulator exception on iOS', () => {
    platform.name = 'ios';
    vi.stubEnv('DEV', false);
    vi.stubEnv('VITE_API_URL', 'http://10.0.2.2:5171/api');
    vi.stubEnv('VITE_ALLOW_INSECURE_LOCAL_API', 'true');

    expect(() => obtener_api_url()).toThrow(/API debe usar HTTPS/);
  });
});
