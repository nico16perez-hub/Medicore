const CLAVE_TEMA = 'medicore_tema';

export function es_tema_oscuro() {
  return localStorage.getItem(CLAVE_TEMA) !== 'claro';
}

export function aplicar_tema_guardado() {
  document.documentElement.classList.toggle('ion-palette-dark', es_tema_oscuro());
}

export function alternar_tema() {
  localStorage.setItem(CLAVE_TEMA, es_tema_oscuro() ? 'claro' : 'oscuro');
  aplicar_tema_guardado();
}