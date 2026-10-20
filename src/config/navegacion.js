import {
  homeOutline,
  calendarOutline,
  documentTextOutline,
  leafOutline,
  notificationsOutline,
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
    roles: [],
    componente: () => import('@/views/inicio_page.vue')
  },
  {
    id: 'turnos',
    titulo: 'Mis Turnos',
    ruta: '/app/turnos',
    icono: calendarOutline,
    grupo_menu: 'operacion',
    orden: 20,
    roles: ['paciente', 'doctor', 'secretaria'],
    componente: () => import('@/views/turnos_page.vue')
  },
  {
    id: 'notificaciones',
    titulo: 'Notificaciones',
    ruta: '/app/notificaciones',
    icono: notificationsOutline,
    grupo_menu: 'operacion',
    orden: 25,
    roles: [],
    componente: () => import('@/views/notificaciones_page.vue')
  },
  {
    id: 'historia',
    titulo: 'Historia Clínica',
    ruta: '/app/historia',
    icono: documentTextOutline,
    grupo_menu: 'operacion',
    orden: 30,
    roles: ['paciente', 'doctor'],
    componente: () => import('@/views/historia_page.vue')
  },
  {
    id: 'nutricion',
    titulo: 'Nutrición',
    ruta: '/app/nutricion',
    icono: leafOutline,
    grupo_menu: 'operacion',
    orden: 40,
    roles: ['paciente', 'doctor'],
    componente: () => import('@/views/nutricion_page.vue')
  },
  {
    id: 'perfil',
    titulo: 'Mi cuenta',
    ruta: '/app/perfil',
    icono: personCircleOutline,
    grupo_menu: 'configuracion',
    orden: 100,
    roles: [],
    componente: () => import('@/views/perfil_page.vue')
  }
];

function obtener_items_visibles(rol){
  return navegacion
    .filter(item => !item.roles.length || item.roles.includes(rol))
    .sort((item_a, item_b) => item_a.orden - item_b.orden);
}

export function obtener_grupos_menu(rol){
  const items = obtener_items_visibles(rol);
  let grupos = grupos_menu.map(grupo => ({
    ...grupo,
    items: items.filter(item => item.grupo_menu === grupo.id)
  }));
  grupos = grupos.filter(grupo => grupo.items.length > 0);
  grupos.sort((grupo_a, grupo_b) => grupo_a.orden - grupo_b.orden);
  return grupos;
}

export function obtener_tabs(rol){
  return obtener_items_visibles(rol);
}