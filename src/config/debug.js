import { Capacitor } from '@capacitor/core';

const HOSTS_HTTP_DESARROLLO = new Set([
  'localhost',
  '127.0.0.1',
  '::1',
  '0.0.0.0'
]);

function parsear_url_api(valor) {
  let url;
  try {
    url = new URL(valor);
  } catch {
    throw new Error('La URL de la API no es válida.');
  }
  return url;
}

function es_ip_privada_ipv4(hostname) {
  const octetos = hostname.split('.');
  if (octetos.length !== 4 || octetos.some(octeto => !/^\d{1,3}$/.test(octeto))) {
    return false;
  }

  const [primero, segundo] = octetos.map(Number);
  if (octetos.some(octeto => Number(octeto) > 255)) {
    return false;
  }

  return primero === 10 ||
    (primero === 172 && segundo >= 16 && segundo <= 31) ||
    (primero === 192 && segundo === 168);
}

function normalizar_url_api(valor) {
  const url = parsear_url_api(valor);
  if (url.protocol === 'https:') {
    return url.toString().replace(/\/$/, '');
  }
  const httpLocalEnDesarrollo = import.meta.env.DEV &&
    HOSTS_HTTP_DESARROLLO.has(url.hostname.toLowerCase());
  const httpEmuladorAndroid = import.meta.env.DEV ||
    import.meta.env.VITE_ALLOW_INSECURE_LOCAL_API === 'true';
  const hostname = url.hostname.toLowerCase();
  const httpAndroidPermitido = httpEmuladorAndroid &&
    Capacitor.getPlatform() === 'android' &&
    (hostname === '10.0.2.2' || es_ip_privada_ipv4(hostname));
  if (url.protocol === 'http:' && (httpLocalEnDesarrollo || httpAndroidPermitido)) {
    return url.toString().replace(/\/$/, '');
  }
  if (url.protocol === 'http:' && Capacitor.getPlatform() === 'web') {
    throw new Error(
      'En navegador, HTTP solo se permite para localhost. Si usas ionic serve en esta PC, detené Vite y ejecutá en la terminal del frontend: $env:VITE_API_URL = "http://localhost:5171/api"; npm run dev. Revisá también .env: VITE_API_URL puede estar configurada allí para Android. Para otro dispositivo u origen, usa una API HTTPS.'
    );
  }
  throw new Error('La API debe usar HTTPS. HTTP solo se permite en loopback o, con la opción de desarrollo activada, en una IP privada desde Android.');
}

export function obtener_api_url() {
  const configurada = (import.meta.env.VITE_API_URL || import.meta.env.VITE_API_URL_DEBUG || '').trim();

  if (configurada) {
    const url = parsear_url_api(configurada);
    if (Capacitor.isNativePlatform() && import.meta.env.DEV &&
        url.protocol === 'http:' && HOSTS_HTTP_DESARROLLO.has(url.hostname.toLowerCase())) {
      return 'http://10.0.2.2:5171/api';
    }
    return normalizar_url_api(configurada);
  }

  if (Capacitor.isNativePlatform()) {
    if (import.meta.env.DEV) {
      return 'http://10.0.2.2:5171/api';
    }
    throw new Error('Configura VITE_API_URL con el endpoint HTTPS de la API para la aplicación nativa.');
  }

  if (typeof window !== 'undefined') {
    const { hostname } = window.location;

    if (hostname && !HOSTS_HTTP_DESARROLLO.has(hostname.toLowerCase())) {
      return normalizar_url_api(`https://${hostname}:5172/api`);
    }

    return normalizar_url_api('http://localhost:5171/api');
  }

  if (import.meta.env.DEV) {
    return normalizar_url_api('http://localhost:5171/api');
  }
  throw new Error('Configura VITE_API_URL con una URL HTTPS para la API.');
}
