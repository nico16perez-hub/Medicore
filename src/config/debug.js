export function obtener_api_url() {
  return (import.meta.env.VITE_API_URL_DEBUG || '').replace(/\/$/, '');
}
