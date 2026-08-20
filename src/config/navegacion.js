import {
  homeOutline,
  calendarOutline,
  documentTextOutline,
  leafOutline,
  personCircleOutline
} from 'ionicons/icons';


export const grupos_menu = [
  {
    id: 'operacion',
    titulo: '',
    orden: 10
  },
  {
    id: 'configuracion',
    titulo: 'Configuración',
    orden: 20
  }
];

export const navegacion = [
  {
    id: 'inicio',
    titulo: 'Inicio',
    ruta: '/app/inicio',
    icono: homeOutline,
    grupo_menu: 'operacion',
    orden: 10,
    componente: () => import('@/views/inicio_page.vue')
  },
  {
    id: 'turnos',
    titulo: 'Mis Turnos',
    ruta: '/app/turnos',
    icono: calendarOutline,
    grupo_menu: 'operacion',
    orden: 20,
    componente: () => import('@/views/turnos_page.vue')
  },
  {
    id: 'historia',
    titulo: 'Historia Clínica',
    ruta: '/app/historia',
    icono: documentTextOutline,
    grupo_menu: 'operacion',
    orden: 30,
    componente: () => import('@/views/historia_page.vue')
  },
  {
    id: 'nutricion',
    titulo: 'Nutrición',
    ruta: '/app/nutricion',
    icono: leafOutline,
    grupo_menu: 'operacion',
    orden: 40,
    componente: () => import('@/views/nutricion_page.vue')
  },
  {
    id: 'perfil',
    titulo: 'Mi cuenta',
    ruta: '/app/perfil',
    icono: personCircleOutline,
    grupo_menu: 'configuracion',
    orden: 100,
    componente: () => import('@/views/perfil_page.vue')
  }
];

export function obtener_grupos_menu(){
  const items = [...navegacion].sort((item_a, item_b) => item_a.orden - item_b.orden);
  let grupos = grupos_menu.map(grupo => ({
    ...grupo,
    items: items.filter(item => item.grupo_menu === grupo.id)
  }));
  grupos = grupos.filter(grupo => grupo.items.length > 0);
  grupos.sort((grupo_a, grupo_b) => grupo_a.orden - grupo_b.orden);
  return grupos;
}

export function obtener_tabs(){
  return [...navegacion].sort((item_a, item_b) => item_a.orden - item_b.orden);
}